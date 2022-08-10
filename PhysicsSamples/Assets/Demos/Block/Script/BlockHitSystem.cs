using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Rendering;
using UnityEngine;

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
        Dependency = new BlockCollisionEventsJob
        {
            hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
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
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    private struct BlockCollisionEventsJob : ICollisionEventsJob
    {
        public ComponentDataFromEntity<URPMaterialPropertyEmissionColor> hdrColorGroup;
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;

            hdrColorGroup[entityB] = new URPMaterialPropertyEmissionColor
            {
                Value = new float4(1, 1, 1, 1)
            };
        }
    }
}
