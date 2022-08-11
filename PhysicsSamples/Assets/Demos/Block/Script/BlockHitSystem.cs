using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;

using System.Collections.Generic;
using Unity.Collections;

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

    struct TweenBase
    {
        public float PassTime;
    }
    List<TweenBase> TweenList = new List<TweenBase>();
    protected override void OnUpdate()
    {
        if (blockGroup.CalculateEntityCount() == 0)
        {
            return;
        }
        var cap = blockGroup.CalculateEntityCount() / 2;
        NativeList<Entity> tweenTarget = new NativeList<Entity>(cap, Allocator.TempJob);

        Dependency = new BlockCollisionEventsJob
        {
            hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
            blockGroup = GetComponentDataFromEntity<BlockComponent>() ,
            bulletGroup = GetComponentDataFromEntity<BulletComponent>() ,
            tweenTarget = tweenTarget,
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);

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
        Dependency.Complete();
        var length = tweenTarget.Length;
        for (int i = 0; i < length; i++)
        {
            var hdrColor = EntityManager.GetComponentData<URPMaterialPropertyEmissionColor>(tweenTarget[i]).Value;
            TweenSystem.CreateTween(tweenTarget[i], hdrColor, new float4(1, 1, 1, 1), 0.1f, DG.Tweening.Ease.Linear, true);
        }
        tweenTarget.Dispose();
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    private struct BlockCollisionEventsJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<URPMaterialPropertyEmissionColor> hdrColorGroup;
        public ComponentDataFromEntity<BlockComponent> blockGroup;
        public ComponentDataFromEntity<BulletComponent> bulletGroup;
        public NativeList<Entity> tweenTarget;
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;


            bool isABullet = bulletGroup.HasComponent(entityA);
            bool isBBullet = bulletGroup.HasComponent(entityB);

            if (isABullet)
            {
                var block = blockGroup[entityB];
                block.HitCountDown -= bulletGroup[entityA].Damage;
                blockGroup[entityB] = block;

                if (block.HitCountDown <= 0)
                {
                }
                else
                {
                    tweenTarget.Add(entityB);
                }
            }
            if (isBBullet)
            {
                var block = blockGroup[entityA];
                block.HitCountDown -= bulletGroup[entityB].Damage;
                blockGroup[entityA] = block;

                if (block.HitCountDown <= 0)
                {
                }
                else
                {
                    tweenTarget.Add(entityA);
                }
            }
        }
    }
}
