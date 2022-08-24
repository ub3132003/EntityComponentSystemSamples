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
            .ForEach((Entity e, in DynamicBuffer<HealthEvent> triggerEventBuffer, in ExplodeComponent explode, in Translation t, in Rotation r) =>
        {
            for (int i = 0; i < triggerEventBuffer.Length; i++)
            {
                if (triggerEventBuffer[i].state != TRIGGER.RISING_EDGE)
                {
                    continue;
                }
                var distanceHits = new NativeList<DistanceHit>(8, Allocator.Temp);

                if (physicsWorld.CollisionWorld.OverlapSphere(t.Value, explode.ExplodeHalfRange, ref distanceHits, explode.Filter))
                {
                    //Debug.Log($"hit: {distanceHits[0].Entity}");
                    for (int j = 0; j < distanceHits.Length; j++)
                    {
                        var other = distanceHits[j].Entity;
                        if (HasComponent<PhysicsVelocity>(other))
                        {
                            //var pv = GetComponent<PhysicsVelocity>(other);
                            //var pm = GetComponent<PhysicsMass>(other);
                            //var force = new float3(0, 10, 0);
                            //var tOther = GetComponent<Translation>(other);
                            //var rOther = GetComponent<Rotation>(other);
                            //pv.ApplyImpulse(pm, tOther, rOther, force, distanceHits[j].Position);

                            //SetComponent(other, pv
                        }
                        if (HasComponent<Health>(other))
                        {
                            //TODO 解耦伤害计算?
                            var hp = GetComponent<Health>(other);
                            if (hp.Value > 0)
                            {
                                hp.Value -= explode.DamageValue;
                                SetComponent(other, hp);
                            }
                        }
                    }
                }

                //销毁爆炸方块
                SetComponent<Health>(e, new Health { Value = 0});
            }
        }).Schedule();


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
