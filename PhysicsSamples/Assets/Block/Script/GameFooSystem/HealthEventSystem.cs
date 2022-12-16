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
/// <summary>
/// 重置血量时间，如果需要用到血量时间需要在此系统之前判断
/// </summary>
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
        //爆炸方块动画
        Entities
            .WithAll<HealthEventHdrAnimTag>()
            .WithoutBurst()
            .WithStructuralChanges()
            .ForEach((Entity e, in DynamicBuffer<HealthEvent> triggerEventBuffer) =>
            {
                if (triggerEventBuffer[0].state == TRIGGER.RISING_EDGE)
                {
                    if (HasComponent<EmissionVector4Override>(e))
                    {
                        var intensity = 4f;
                        var hdrColor = GetComponent<EmissionVector4Override>(e);
                        float factor = Mathf.Pow(2, intensity);
                        var tween = new TweenData(TypeOfTween.HdrColor, e, hdrColor.Value * factor, 1)
                            .FromValue(hdrColor.Value)
                            .SetEase(DG.Tweening.Ease.OutSine);
                        TweenCreateSystem.AddTweenComponent<TweenHDRColorComponent>(commandBuffer, tween);
                        //ITweenComponent.CreateHdrColorTween(e, hdrColor.Value, hdrColor.Value * factor, 1, DG.Tweening.Ease.OutSine);
                    }
                }
            }).Run();

        //爆炸方块触发
        var physicsWorld = m_BuildPhysicsWorld.PhysicsWorld;
        Entities
            .ForEach((Entity e, ref DynamicBuffer<AbillitySpawnComponent> abBuffer, in DynamicBuffer<HealthEvent> triggerEventBuffer,  in Translation t, in Rotation r) =>
        {
            for (int i = 0; i < triggerEventBuffer.Length; i++)
            {
                if (triggerEventBuffer[i].state != TRIGGER.RISING_EDGE)
                {
                    continue;
                }
                for (int k = 0; k < abBuffer.Length; k++)
                {
                    var ab = abBuffer[k];
                    ab.StartTime = time + ab.Delay;
                    abBuffer[k] = ab;
                }
            }
        }).Schedule();

        // 技能生成
        Entities
            .ForEach((Entity e, ref DynamicBuffer<AbillitySpawnComponent> abBuffer, in Translation translation) =>
        {
            for (int k = 0; k < abBuffer.Length; k++)
            {
                var ab = abBuffer[k];
                if (time > ab.StartTime && ab.StartTime != 0)
                {
                    //SetComponent(ab.Target, new Health { Value = 0 });
                    var abEntity = commandBuffer.Instantiate(ab.Abillity);
                    commandBuffer.SetComponent(abEntity, translation);

                    commandBuffer.AddComponent(abEntity, new Abillity { Caster = e });
                    //if(ab.TargetType == AbillitySpawnComponent.TARGET_TYPES.TARGET_PROJECTILE
                    //||ab.TargetType == AbillitySpawnComponent.TARGET_TYPES.TARGET_INSTANT)
                    //{
                    //    if (HasComponent<TargetInstant>(abEntity))
                    //    {
                    //        commandBuffer.SetComponent(abEntity,new TargetInstant { Target = })
                    //    }
                    //}
                    ab.StartTime = 0;
                }
                abBuffer[k] = ab;
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
