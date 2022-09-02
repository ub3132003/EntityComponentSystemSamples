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
using Unity.Physics.Extensions;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(Unity.Physics.Extensions.MousePickSystem)) , UpdateAfter(typeof(HealthSystem))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class BlockHitSystem : SystemBase
{
    EntityQuery blockGroup;
    EntityQuery bulletGroup;
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    BuildPhysicsWorld buildPhysicsWorld;


    //EndSimulationEntityCommandBufferSystem endSimulationEcbSystem;
    protected override void OnCreate()
    {
        buildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();

        //endSimulationEcbSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();

        blockGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(BrickComponent)
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
            blockGroup = GetComponentDataFromEntity<BrickComponent>() ,
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
            //问题，从原色改变时，短时间多次改变会累加值无法记录原始值，  对于原本已经有hdr颜色，无法做到闪白恢复效果。
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


        //查找死亡方块
        NativeList<DeadBrickData> deadBrickDatas = new NativeList<DeadBrickData>(capBlock / 2, Allocator.TempJob);
        Entities
            .ForEach((Entity e, in Health health , in BrickComponent brick , in  LocalToWorld localToWorld) =>
        {
            if (health.Value <= 0)
            {
                var deadData = new DeadBrickData
                {
                    position = localToWorld.Position,
                    dropCount = brick.DieDropCount,
                };
                deadBrickDatas.Add(deadData);
            }
        }).Schedule();
        Dependency.Complete();

        //删除 死亡实体,记录死亡位置
        length = deadBrickDatas.Length;
        //mono 通讯
        PlayerEcsConnect.Instance.AddEXP(length);
        if (length == 0)
        {
            contactData.Dispose();
            tweenTarget.Dispose();
            deadBlocks.Dispose();
            deadBrickDatas.Dispose();
            return;
        }

        //播放死亡时粒子
        Entities.WithoutBurst().WithAll<BrickDeadPsTag>().ForEach((UnityEngine.ParticleSystem ps, in BrickDeadPsTag psTag) =>
        {
            for (int i = 0; i < length; i++)
            {
                ps.transform.position = deadBrickDatas[i].position;
                int n = deadBrickDatas[i].dropCount;
                if (!psTag.IsEmit)
                {
                    ps.Emit(n);
                }

                ps.Play();
            }
        }).Run();


        tweenTarget.Dispose();
        contactData.Dispose();
        deadBrickDatas.Dispose();
        //toFallBlocks.Dispose();
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
        public ComponentDataFromEntity<BrickComponent> blockGroup;
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
            tweenTarget.Add(blockEntity);
            //var block = blockGroup[blockEntity];
            //block.HitCountDown -= bulletGroup[bulletEntity].Damage;
            //blockGroup[blockEntity] = block;
            //if (block.HitCountDown == 0)
            //{
            //    deadBlocks.Add(blockEntity);
            //}
            //else if (block.HitCountDown < 0)
            //{
            //    //一帧中同时命中,导致hcountdown 小于0 避免重复添加
            //    if (!deadBlocks.Contains(blockEntity))
            //    {
            //        deadBlocks.Add(blockEntity);
            //    }
            //    var manager = World.DefaultGameObjectInjectionWorld.EntityManager;
            //    if (manager.HasComponent<BulletRockTag>(bulletEntity))
            //    {
            //        //继续前进而不是反弹
            //        var I = PhysicsVelocityGroup[bulletEntity].Linear;
            //        var N = collisionEvent.Normal;
            //        var R = I - math.dot(N, I) * N * 2.0f;
            //        PhysicsVelocityGroup[bulletEntity] = new PhysicsVelocity
            //        {
            //            Linear = R,
            //            Angular = R / 1 // 线速度 除 半径
            //        };
            //        //var pv = PhysicsVelocityGroup[bulletEntity];
            //        //pv.ApplyImpulse(
            //        //    manager.GetComponentData<PhysicsMass>(bulletEntity),
            //        //    manager.GetComponentData<Translation>(bulletEntity),
            //        //    manager.GetComponentData<Rotation>(bulletEntity),
            //        //    R, float3.zero);
            //    }
            //}
            //else
            //{

            //}
        }
    }
}
