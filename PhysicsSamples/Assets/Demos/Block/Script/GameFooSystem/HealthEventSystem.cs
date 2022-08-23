using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;

public partial class HealthEventSystem : SystemBase
{
    private EntityQuery m_Query = default;
    private StatVitalityEventBuffers<Health> m_StateFulEventBuffers;
    private EndFixedStepSimulationEntityCommandBufferSystem m_CommandBufferSystem;

    protected override void OnCreate()
    {
        m_Query = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(HealthEvent)
            }
        });

        m_StateFulEventBuffers = new StatVitalityEventBuffers<Health>();
        m_CommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
    }

    protected override void OnDestroy()
    {
        m_StateFulEventBuffers.Dispose();
    }

    protected override void OnUpdate()
    {
        if (m_Query.CalculateEntityCount() == 0)
        {
            return;
        }
        //Entities

        //    .WithBurst()
        //    .ForEach((ref DynamicBuffer<HealthEvent> buffer) =>
        //    {
        //        buffer.Clear();
        //    }).ScheduleParallel();

        //m_StateFulEventBuffers.SwapBuffers();
        //var currentEvents = m_StateFulEventBuffers.Current;
        //var previousEvents = m_StateFulEventBuffers.Previous;

        var commandBuffer = m_CommandBufferSystem.CreateCommandBuffer();

        Entities
            .WithName("HealthEventJOb")
            .WithBurst()
            .WithChangeFilter<Health>()
            .ForEach((DynamicBuffer<HealthEvent> healthEventBuffers) =>
            {
                for (int i = 0; i < healthEventBuffers.Length; i++)
                {
                    int a = (int)healthEventBuffers[i].state;
                    Debug.Log($"state: {a}");
                }
                //var statefulEvents = new NativeList<Health>(currentEvents.Length, Allocator.Temp);

                //StatVitalityEventBuffers<Health>.GetStatefulEvents(previousEvents, currentEvents, statefulEvents);

                //for (int i = 0; i < statefulEvents.Length; i++)
                //{
                //}
            }).Schedule();

        Entities
            .WithName("ChangeMaterialOnHealthLessJob")
            .WithoutBurst()
            .ForEach((Entity e, ref DynamicBuffer<HealthEvent> triggerEventBuffer, in TriggerHealthChangeMaterial changeMaterial) =>
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    if (triggerEventBuffer[i].state == TRIGGER.RISING_EDGE)
                    {
                        var volumeRenderMesh = EntityManager.GetSharedComponentData<RenderMesh>(e);
                        volumeRenderMesh.material = changeMaterial.material;
                        commandBuffer.SetSharedComponent(e, volumeRenderMesh);
                    }
                }
            }).Run();
        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
