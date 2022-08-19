using Unity.Burst;
using Unity.Transforms;
using Unity.Physics;
using Unity.Entities;
using Unity.Mathematics;

using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;

using System.Collections.Generic;
using Unity.Collections;
using Unity.Physics.Stateful;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(Unity.Physics.Extensions.MousePickSystem))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class BlockHitSystem : SystemBase
{
    EntityQuery blockGroup;
    EntityQuery bulletGroup;
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    BuildPhysicsWorld buildPhysicsWorld;


    EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

        endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        blockGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(BlockComponent)
            }
        });
        bulletGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(BulletComponent)
            }
        });
    }

    struct DeadBrickData
    {
        public float3 position;
        public int dropCount;
    }
    struct ContactPointData
    {
        public float3 point;
        public float3 normal;

        public int PsId1, PsId2;
    }
    protected override void OnUpdate()
    {
        if (blockGroup.CalculateEntityCount() == 0)
        {
            return;
        }
        var capBlock = blockGroup.CalculateEntityCount();
        var capBullet = bulletGroup.CalculateEntityCount();

        NativeList<ContactPointData> contactData = new NativeList<ContactPointData>(capBullet, Allocator.TempJob);
        NativeList<Entity> tweenTarget = new NativeList<Entity>(capBlock / 2, Allocator.TempJob);
        NativeList<Entity> deadBlocks = new NativeList<Entity>(capBlock / 2, Allocator.TempJob);

        Dependency = new BlockCollisionEventsJob
        {
            PhysicsWorld = buildPhysicsWorld.PhysicsWorld,
            collisionDatas = contactData,
            hitPsGroutp = GetComponentDataFromEntity<ColliderHitPsTag>(),
            blockGroup = GetComponentDataFromEntity<BlockComponent>() ,
            bulletGroup = GetComponentDataFromEntity<BulletComponent>() ,
            PhysicsVelocityGroup = GetComponentDataFromEntity<PhysicsVelocity>(),
            tweenTarget = tweenTarget,
            deadBlocks = deadBlocks,
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);
        Dependency.Complete();


        //加入 变色动画
        var length = tweenTarget.Length;
        for (int i = 0; i < length; i++)
        {
            //从全白渐变到无hdr
            ITweenComponent.CreateTween(tweenTarget[i],  new float4(1, 1, 1, 1), float4.zero, 0.1f, DG.Tweening.Ease.Linear);
        }

        //命中效果
        length = contactData.Length;
        for (int i = 0; i < length; i++)
        {
            Entities.WithoutBurst().ForEach((UnityEngine.ParticleSystem ps , in HitPsTag hitPs) =>
            {
                var hitInfo = contactData[i];
                if (hitInfo.PsId1 == ps.GetInstanceID() || hitInfo.PsId2 == ps.GetInstanceID())
                {
                    ps.transform.position = hitInfo.point;
                    ps.transform.forward = hitInfo.normal;
                    ps.Play();
                }
            }).Run();
        }


        //删除 死亡实体,记录死亡位置
        length = deadBlocks.Length;
        //mono 通讯
        PlayerEcsConnect.Instance.AddEXP(length);
        if (length == 0)
        {
            contactData.Dispose();
            tweenTarget.Dispose();
            deadBlocks.Dispose();
            return;
        }

        NativeList<DeadBrickData> deadBrickDatas = new NativeList<DeadBrickData>(capBlock / 2, Allocator.TempJob);
        using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
        {
            for (int i = 0; i < length; i++)
            {
                var deadData = new DeadBrickData
                {
                    position = EntityManager.GetComponentData<Translation>(deadBlocks[i]).Value,
                    dropCount = EntityManager.GetComponentData<BlockComponent>(deadBlocks[i]).DieDropCount,
                };

                deadBrickDatas.Add(deadData);
                //EntityManager.DestroyEntity(deadBlocks[i]);
                commandBuffer.DestroyEntity(deadBlocks[i]);

                //播放死亡时粒子
                Entities.WithoutBurst().WithAll<BrickDeadPsTag>().ForEach((UnityEngine.ParticleSystem ps, in BrickDeadPsTag psTag) =>
                {
                    ps.transform.position = deadBrickDatas[i].position;
                    int n = deadBrickDatas[i].dropCount;
                    if (!psTag.IsEmit)
                    {
                        ps.Emit(n);
                    }

                    ps.Play();
                }).Run();
            }
            commandBuffer.Playback(EntityManager);
        }


        endSimulationEcbSystem.AddJobHandleForProducer(this.Dependency);

        //查找需要下降的砖 比死亡砖块高的
        NativeList<Entity> toFallBlocks = new NativeList<Entity>(capBlock / 2, Allocator.TempJob);
        Entities
            .WithAll<BlockComponent>()
            .ForEach((Entity entity, in Translation t) => {
                var x = t.Value.x;
                var z = t.Value.z;
                var y = t.Value.y;
                var length = deadBrickDatas.Length;
                for (int i = 0; i < length; i++)
                {
                    var deadPos = deadBrickDatas[i].position;
                    if (x == deadPos.x && z == deadPos.z && deadPos.y < y)
                    {
                        toFallBlocks.Add(entity);
                    }
                }
            }).Schedule();


        Dependency.Complete();

        //加入下降动画
        length = toFallBlocks.Length;
        for (int i = 0; i < length; i++)
        {
            ITweenComponent.CreateMoveTween(toFallBlocks[i], math.down(), 1f, DG.Tweening.Ease.InCubic , isRelative: true);
        }

        tweenTarget.Dispose();
        contactData.Dispose();
        deadBrickDatas.Dispose();
        toFallBlocks.Dispose();
        deadBlocks.Dispose();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    private struct BlockCollisionEventsJob : ICollisionEventsJob
    {
        [ReadOnly]
        public PhysicsWorld PhysicsWorld;

        public ComponentDataFromEntity<ColliderHitPsTag> hitPsGroutp;
        public ComponentDataFromEntity<BlockComponent> blockGroup;
        public ComponentDataFromEntity<BulletComponent> bulletGroup;
        public ComponentDataFromEntity<PhysicsVelocity> PhysicsVelocityGroup;

        public NativeList<ContactPointData> collisionDatas;
        public NativeList<Entity> tweenTarget;
        public NativeList<Entity> deadBlocks;
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;


            var colision = collisionEvent.CalculateDetails(ref PhysicsWorld);
            collisionDatas.Add(new ContactPointData
            {
                point = colision.EstimatedContactPointPositions[0],
                normal = collisionEvent.Normal,
                PsId1 = hitPsGroutp.HasComponent(entityA) ? hitPsGroutp[entityA].PsId : 0,
                PsId2 = hitPsGroutp.HasComponent(entityB) ? hitPsGroutp[entityB].PsId : 0,
            });

            bool isABullet = bulletGroup.HasComponent(entityA);
            bool isBBullet = bulletGroup.HasComponent(entityB);

            bool isABlock = blockGroup.HasComponent(entityA);
            bool isBBlock = blockGroup.HasComponent(entityB);

            Entity bulletEntity;
            Entity blockEntity;
            if (isABullet && isBBlock)
            {
                bulletEntity = entityA;
                blockEntity = entityB;
            }
            else if (isABlock && isBBullet)
            {
                bulletEntity = entityB;
                blockEntity = entityA;
            }
            else
            {
                return;
            }
            var block = blockGroup[blockEntity];
            block.HitCountDown -= bulletGroup[bulletEntity].Damage;
            blockGroup[blockEntity] = block;
            //BUg 删除方块后 球不会反弹,手动设置反射速度
            if (block.HitCountDown == 0)
            {
                deadBlocks.Add(blockEntity);
                //var I = PhysicsVelocityGroup[bulletEntity].Linear;
                //var N = collisionEvent.Normal;
                //var R = I - math.dot(N, I) * N * 2.0f;
                //PhysicsVelocityGroup[bulletEntity] = new PhysicsVelocity
                //{
                //    Linear = R,
                //};
            }
            else if (block.HitCountDown < 0)
            {
                //一帧中同时命中,导致hcountdown 小于0 避免重复添加
            }
            else
            {
                tweenTarget.Add(blockEntity);
            }
        }
    }
}
