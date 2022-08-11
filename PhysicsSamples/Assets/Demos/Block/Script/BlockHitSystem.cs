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


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class BlockHitSystem : SystemBase
{
    EntityQuery blockGroup;
    StepPhysicsWorld m_StepPhysicsWorldSystem;

    protected override void OnCreate()
    {
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        blockGroup = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(BlockComponent)
            }
        });
    }

    protected override void OnUpdate()
    {
        if (blockGroup.CalculateEntityCount() == 0)
        {
            return;
        }
        var cap = blockGroup.CalculateEntityCount() / 2;
        NativeList<Entity> tweenTarget = new NativeList<Entity>(cap, Allocator.TempJob);
        NativeList<Entity> deadBlocks = new NativeList<Entity>(cap, Allocator.TempJob);

        Dependency = new BlockCollisionEventsJob
        {
            blockGroup = GetComponentDataFromEntity<BlockComponent>() ,
            bulletGroup = GetComponentDataFromEntity<BulletComponent>() ,
            tweenTarget = tweenTarget,
            deadBlocks = deadBlocks,
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);
        Dependency.Complete();

        //var dt = Time.DeltaTime;
        //Entities.
        //    ForEach((Entity ent, ref URPMaterialPropertyEmissionColor hdr) =>
        //    {
        //        hdr.Value.y += dt;
        //        if (hdr.Value.y > 1)
        //        {
        //            hdr.Value.y = 0;
        //        }
        //    }).Run();


        //加入 变色动画
        var length = tweenTarget.Length;
        for (int i = 0; i < length; i++)
        {
            var hdrColor = EntityManager.GetComponentData<URPMaterialPropertyEmissionColor>(tweenTarget[i]).Value;
            //从全白渐变到无hdr
            TweenComponent.CreateTween(tweenTarget[i],  new float4(1, 1, 1, 1), float4.zero, 0.1f, DG.Tweening.Ease.Linear);
        }
        tweenTarget.Dispose();


        //删除 死亡实体,记录死亡位置
        length = deadBlocks.Length;
        if (length == 0) { deadBlocks.Dispose(); return; }

        NativeList<float3> deadBlockPositions = new NativeList<float3>(cap, Allocator.TempJob);
        for (int i = 0; i < length; i++)
        {
            var position = EntityManager.GetComponentData<Translation>(deadBlocks[i]).Value;
            deadBlockPositions.Add(position);
            EntityManager.DestroyEntity(deadBlocks[i]);
        }
        deadBlocks.Dispose();


        //查找需要下降的砖 比死亡砖块高的
        NativeList<Entity> toFallBlocks = new NativeList<Entity>(cap, Allocator.TempJob);
        Entities
            .WithAll<BlockComponent>()
            .WithDisposeOnCompletion(deadBlockPositions)
            .ForEach((Entity entity, in Translation t) => {
                var x = t.Value.x;
                var z = t.Value.z;
                var y = t.Value.y;
                var length = deadBlockPositions.Length;
                for (int i = 0; i < length; i++)
                {
                    var deadPos = deadBlockPositions[i];
                    if (x == deadPos.x && z == deadPos.z && deadPos.y < y)
                    {
                        toFallBlocks.Add(entity);
                    }
                }
            }).Run();


        Dependency.Complete();

        //加入下降动画
        length = toFallBlocks.Length;
        for (int i = 0; i < length; i++)
        {
            TweenComponent.CreateTween(toFallBlocks[i], math.down(), 1f, DG.Tweening.Ease.OutElastic , isRelative: true);
        }
        toFallBlocks.Dispose();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    private struct BlockCollisionEventsJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<BlockComponent> blockGroup;
        public ComponentDataFromEntity<BulletComponent> bulletGroup;
        public NativeList<Entity> tweenTarget;
        public NativeList<Entity> deadBlocks;
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;


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

            if (block.HitCountDown <= 0)
            {
                deadBlocks.Add(blockEntity);
            }
            else
            {
                tweenTarget.Add(blockEntity);
            }
        }
    }
}
