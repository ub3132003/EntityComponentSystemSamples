using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Rendering;
using Unity.Transforms;
using UnityEngine;
[UpdateAfter(typeof(HealthSystem))]
public partial class HealthEventSystem : SystemBase
{
    private EntityQuery m_Query = default;
    private StatVitalityEventBuffers<Health> m_StateFulEventBuffers;
    private EndFixedStepSimulationEntityCommandBufferSystem m_CommandBufferSystem;
    private BuildPhysicsWorld m_BuildPhysicsWorld;

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

        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
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

        var time = Time.ElapsedTime;

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
        //低血量材质提示
        Entities
            .WithName("ChangeMaterialOnHealthLessJob")
            .WithoutBurst()
            .ForEach((Entity e, in DynamicBuffer<HealthEvent> triggerEventBuffer, in TriggerHealthChangeMaterial changeMaterial) =>
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

        //爆炸方块触发
        var physicsWorld = m_BuildPhysicsWorld.PhysicsWorld;
        Entities
            .ForEach((Entity e, ref AbillitySpawnComponent ab, in DynamicBuffer<HealthEvent> triggerEventBuffer,  in Translation t, in Rotation r) =>
        {
            for (int i = 0; i < triggerEventBuffer.Length; i++)
            {
                if (triggerEventBuffer[i].state != TRIGGER.RISING_EDGE)
                {
                    continue;
                }
                ab.StartTime = time + ab.Delay;
            }
        }).Schedule();

        //执行技能
        Entities
            .ForEach((ref AbillitySpawnComponent ab , in Translation translation) =>
        {
            if (time > ab.StartTime && ab.StartTime != 0)
            {
                //SetComponent(ab.Target, new Health { Value = 0 });
                var abEntity = commandBuffer.Instantiate(ab.Abillity);
                commandBuffer.SetComponent(abEntity, translation);
                ab.StartTime = 0;
            }
        }).Schedule();
        m_CommandBufferSystem.AddJobHandleForProducer(Dependency);

        //TODO 改为缓存当前,在下一帧重置
        //更新血量触发事件
        Entities
            .ForEach((ref DynamicBuffer<HealthEvent> eventBuffers, in Health health) =>
        {
            for (int j = 0; j < eventBuffers.Length; j++)
            {
                eventBuffers[j] = HealthEvent.CompareEvent(eventBuffers[j], health.Value);
            }
        }).Schedule();
    }
}
