using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;
using DG.Tweening.Core.Easing;
using DG.Tweening;
using System.Collections.Generic;

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
        Dependency = new BlockCollisionEventsJob
        {
            hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
            blockGroup = GetComponentDataFromEntity<BlockComponent>() ,
            bulletGroup = GetComponentDataFromEntity<BulletComponent>() ,
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
        var detleTime = Time.DeltaTime;
        var length = TweenList.Count;
        for (int i = 0; i < length; i++)
        {
            var tween = TweenList[i];
            tween.PassTime += detleTime;
            var v = EaseManager.Evaluate(Ease.Linear, null, tween.PassTime, 0.5f, 0, 0);
            //item.DropItemTransform.position = Vector3.Lerp(item.DropItemTransform.position, playPosition, v);
        }
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
                    hdrColorGroup[entityB] = new URPMaterialPropertyEmissionColor
                    {
                        Value = new float4(1, 1, 1, 1)
                    };
                }
            }
            if (isBBullet)
            {
                var block = blockGroup[entityA];
                block.HitCountDown -= bulletGroup[entityB].Damage;
                blockGroup[entityA] = block;

                if (block.HitCountDown <= 0)
                {
                    hdrColorGroup[entityA] = new URPMaterialPropertyEmissionColor
                    {
                        Value = new float4(1, 1, 1, 1)
                    };
                }
            }
        }
    }
}
