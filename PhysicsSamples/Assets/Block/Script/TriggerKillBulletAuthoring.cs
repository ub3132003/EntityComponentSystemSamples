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
using Tween;

/// <summary>
/// 终点线，代表弹球游戏的界外区域。
/// </summary>
public struct TriggerKillBullet : IComponentData
{
    public Entity Prefab { get; set; }
    public Entity KillBrickPrefab { get; set; }
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

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(KillVfxPrefab);
    }
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
    private EntityQueryMask brickMask;
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
        brickMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BrickEndGame),
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
        var length = 0;
        var cap = bulletQuery.CalculateEntityCount() / 2;

        NativeList<Entity> deadBullets = new NativeList<Entity>(cap, Allocator.TempJob);

        //Dependency = new TriggerKillBulletJob
        //{
        //    BulletGroup = GetComponentDataFromEntity<BulletComponent>(true),
        //    KillBulletGroup = GetComponentDataFromEntity<TriggerKillBullet>(),
        //    DeadBullet = deadBullets
        //}.Schedule(m_StepPhysicsWorldSystem.Simulation, Dependency);

        NativeList<float3> deadPositions = new NativeList<float3>(cap, Allocator.TempJob);
        //查找到达界外的方块
        NativeList<Entity> deadBricks = new NativeList<Entity>(3, Allocator.TempJob);

        var bulletMask = this.bulletMask;
        var brickMask = this.brickMask;
        //进如trigger的物体分类添加到数组中
        JobHandle triggerJob = Entities
            .WithName("TriggerKillBullet")
            .ForEach((Entity e, ref TriggerKillBullet triggerKillBullet, in DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer) =>
            {
                triggerKillBullet.Count = 0;
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];

                    var bulletEntity = triggerEvent.GetOtherEntity(e);
                    //  进去 且是 bullet
                    if (triggerEvent.State == StatefulEventState.Enter)
                    {
                        if (bulletMask.Matches(bulletEntity))
                        {
                            triggerKillBullet.Count++;
                            var deadPosition = GetComponent<Translation>(bulletEntity).Value;
                            deadPositions.Add(deadPosition);
                            deadBullets.Add(bulletEntity);
                        }
                    }
                }
            }).Schedule(Dependency);
        triggerJob.Complete();

        //删除子弹, 死亡记录位置


        if (deadBullets.Length > 0)//生成死亡特效 TODO, 使用DynamicBuffer 不同触发不同效果
        {
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
                    if (count <= 0) continue;
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

            length = deadBullets.Length;
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
        }

        NativeList<float3> fallPosition = new NativeList<float3>(length, Allocator.TempJob);
        EntityCommandBufferSystem sys =
            this.World.GetExistingSystem<EndFixedStepSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = sys.CreateCommandBuffer();
        //触发局外方块
        Entities
            .WithoutBurst()
            .WithAll<BrickOutBoundsArea>()
            .ForEach((Entity e, in DynamicBuffer<StatefulTriggerEvent> triggerEventBuffer , in Translation translation) =>
            {
                for (int i = 0; i < triggerEventBuffer.Length; i++)
                {
                    var triggerEvent = triggerEventBuffer[i];

                    var enterEntity = triggerEvent.GetOtherEntity(e);
                    //  进去 且是 endGame
                    if (triggerEvent.State == StatefulEventState.Enter)
                    {
                        if (brickMask.Matches(enterEntity))
                        {
                            deadBricks.Add(enterEntity);
                            var pos = EntityManager.GetComponentData<Translation>(enterEntity);
                            pos.Value.x = math.floor(pos.Value.x);
                            pos.Value.z = math.floor((pos.Value.z + 1));
                            fallPosition.Add(pos.Value);
                            //EntityManager.DestroyEntity(deadBricks[i]);
                            //向前移動
                            //ITweenComponent.CreateMoveTween(e, pos.Value, 2f, DG.Tweening.Ease.InCubic, isRelative: true);
                            translation.DOMove(e, ecb, pos.Value, 2f);
                            Debug.Log($"死方块 {pos.Value}");
                        }
                    }
                }
            }).Run();
        sys.AddJobHandleForProducer(this.Dependency);
        length = deadBricks.Length;
        if (length > 0)
        {
            Unity.Mathematics.Random random = new Unity.Mathematics.Random(444);
            //地面掉落
            float fallTime = 2f;
            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity e, in Translation translation, in BrickFloorComponent floor) =>
                {
                    for (int i = 0; i < fallPosition.Length; i++)
                    {
                        if ((translation.Value.xz == fallPosition[i].xz).IsTure())
                        {
                            ITweenComponent.CreateMoveTween(e, new float3(0, -10, 0), fallTime, DG.Tweening.Ease.InCubic, isRelative: true).SetDelay(e, random.NextFloat() * 5);
                            EntityManager.RemoveComponent<BrickFloorComponent>(e);
                            EntityManager.AddComponentData(e, new LifeTime { Value = 300 });
                        }
                    }
                }).Run();
        }

        fallPosition.Dispose();
        deadBricks.Dispose();
        deadBullets.Dispose();
        deadPositions.Dispose();
    }

    public void TryRemoveComponent<T>(Entity e)
    {
        if (EntityManager.HasComponent<T>(e))
        {
            EntityManager.RemoveComponent<T>(e);
        }
    }
}
