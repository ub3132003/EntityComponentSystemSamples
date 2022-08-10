using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
//public  abstract partial class PhysicsEventSystemBase : SystemBase
//{
//    protected BuildPhysicsWorld BuildPhysicsWorld;
//    protected StepPhysicsWorld m_StepPhysicsWorldSystem;
//    protected EntityCommandBufferSystem CommandBufferSystem;

//    protected override void OnCreate()
//    {
//        BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
//        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
//        CommandBufferSystem = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
//    }
//}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld)), UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class TestPhysicsEventSystem : SystemBase
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
        }.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);
    }

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    private struct BlockCollisionEventsJob : ICollisionEventsJob
    {
        public void Execute(CollisionEvent collisionEvent)
        {
            var entityA = collisionEvent.EntityA;
            var entityB = collisionEvent.EntityB;

            Debug.Log($"A: {entityA}, B: {entityB}");
        }
    }
}
