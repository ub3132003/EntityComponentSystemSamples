using System.Collections.Generic;
using Unity.Collections;
using Unity.Burst;
using Unity.Entities;
using Unity.Jobs;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Stateful;

public struct TriggerKillBullet : IComponentData
{
    public Entity Prefab { get; set; }
    //kill的数量
    public int Count { get; set; }
}

public class TriggerKillBulletAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    /// <summary>
    /// 删除子弹时的特效
    /// </summary>
    public GameObject KillVfxPrefab;

    void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new TriggerKillBullet
        {
            Prefab = conversionSystem.GetPrimaryEntity(KillVfxPrefab),
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(KillVfxPrefab);
}


/// <summary>
/// 触发 删除球,并生成一个死亡效果
/// </summary>
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(ExportPhysicsWorld))]
[UpdateAfter(typeof(StatefulTriggerEventBufferSystem))]
public partial class TriggerKillBulletSystem : SystemBase
{
    StepPhysicsWorld m_StepPhysicsWorldSystem;
    EntityQuery triggerKillBulletQuery;
    EntityQuery bulletQuery;
    private EntityQueryMask bulletMask;
    protected override void OnCreate()
    {
        m_StepPhysicsWorldSystem = World.GetOrCreateSystem<StepPhysicsWorld>();
        triggerKillBulletQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(TriggerKillBullet)
            }
        });
        bulletQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(BulletComponent)
            }
        });

        bulletMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BulletComponent),
                    typeof(Rotation),
                    typeof(PhysicsVelocity)
                },
                None = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                }
            })
        );
    }

    //[BurstCompile]
    //struct TriggerKillBulletJob : ITriggerEventsJob
    //{
    //    [ReadOnly] public ComponentDataFromEntity<BulletComponent> BulletGroup;
    //    public ComponentDataFromEntity<TriggerKillBullet> KillBulletGroup;
    //    public NativeList<Entity> DeadBullet;

    //    public void Execute(TriggerEvent triggerEvent)
    //    {
    //        Entity entityA = triggerEvent.EntityA;
    //        Entity entityB = triggerEvent.EntityB;

    //        bool isBodyATrigger = BulletGroup.HasComponent(entityA);
    //        bool isBodyBTrigger = BulletGroup.HasComponent(entityB);

    //        bool isAKillBullet = KillBulletGroup.HasComponent(entityA);
    //        bool isBKillBullet = KillBulletGroup.HasComponent(entityB);
    //        //// Ignoring Triggers overlapping other Triggers
    //        //if (isBodyATrigger && isBodyBTrigger)
    //        //    return;

    //        //// Ignoring overlapping static bodies
    //        //if ((isBodyATrigger) ||
    //        //    (isBodyBTrigger))
    //        //    return;

    //        var triggerEntity = isBodyATrigger ? entityA : entityB;
    //        var dynamicEntity = isBodyATrigger ? entityB : entityA;
    //    }
    //}

    protected override void OnStartRunning()
    {
        base.OnStartRunning();
        this.RegisterPhysicsRuntimeSystemReadOnly();
    }

    protected override void OnUpdate()
    {
        if (triggerKillBulletQuery.CalculateEntityCount() == 0)
        {
            return;
        }

        var cap = bulletQuery.CalculateEntityCount() / 2;

        NativeList<Entity> deadBullets = new NativeList<Entity>(cap, Allocator.TempJob);
        //Dependency = new TriggerKillBulletJob
        //{
        //    BulletGroup = GetComponentDataFromEntity<BulletComponent>(true),
        //    KillBulletGroup = GetComponentDataFromEntity<TriggerKillBullet>(),
        //    DeadBullet = deadBullets
        //}.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);

        NativeList<float3> deadPositions = new NativeList<float3>(cap, Allocator.TempJob);
        var bulletMask = this.bulletMask;

        JobHandle triggerJob = Entities
            .WithName("TriggerKillBullet")
            .WithoutBurst()
            .ForEach((Entity e, ref DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer, ref TriggerKillBullet killBullet) =>
            {
                killBullet.Count = 0;
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];

                    var bulletEntity = triggerEvent.GetOtherEntity(e);
                    //  进去 且是 bullet
                    if (triggerEvent.State == StatefulEventState.Enter && bulletMask.Matches(bulletEntity))
                    {
                        killBullet.Count++;
                        var deadPosition = GetComponent<Translation>(bulletEntity).Value;
                        deadPositions.Add(deadPosition);
                        deadBullets.Add(bulletEntity);
                    }
                }
            }).Schedule(Dependency);
        triggerJob.Complete();
        //删除子弹, 死亡记录位置

        var length = deadBullets.Length;
        if (length == 0)
        {
            deadBullets.Dispose();
            deadPositions.Dispose();
            return;
        }

        for (int i = 0; i < length; i++)
        {
            var ball = deadBullets[i];
            //收集阳光
            if (HasComponent<SunCoinComponent>(ball))
            {
                var sunCoin = GetComponent<SunCoinComponent>(ball);
                BallAbillityManager.Instance.SunCoinNum += sunCoin.Value;
            }
            EntityManager.DestroyEntity(deadBullets[i]);
        }
        deadBullets.Dispose();

        //生成死亡特效 TODO, 使用DynamicBuffer 不同触发不同效果
        using (var entities = GetEntityQuery(new ComponentType[] { typeof(TriggerKillBullet) }).ToEntityArray(Allocator.TempJob))
        {
            var deadIndex = 0;
            for (int j = 0; j < entities.Length; j++)
            {
                var entity = entities[j];
                var spawnSettings = EntityManager.GetComponentData<TriggerKillBullet>(entity);

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // Limit the number of bodies on platforms with potentially low-end devices
                var count = math.min(spawnSettings.Count, 500);
#else
                var count = spawnSettings.Count;
#endif

                var instances = new NativeArray<Entity>(count, Allocator.Temp);
                EntityManager.Instantiate(spawnSettings.Prefab, instances);

                for (int i = 0; i < count; i++)
                {
                    var instance = instances[i];
                    EntityManager.SetComponentData(instance, new Translation { Value = deadPositions[deadIndex + i] });
                }
                deadIndex += count;
            }
        }
        deadPositions.Dispose();
    }
}
