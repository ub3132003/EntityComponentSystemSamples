using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;


[Serializable]
public struct BrickComponent : IComponentData
{
    //倒数命中计数，0时爆碎
    //public int HitCountDown;

    /// <summary>
    /// 死亡时掉落金币数量
    /// </summary>
    public int DieDropCount;
}

public class BrickComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public int HitCountDown;

    public int DieDropCount;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BrickComponent
        {
            DieDropCount = DieDropCount,
        });
        dstManager.AddComponentData(entity, new FallDownComponent());

        dstManager.AddComponentData(entity, new Health
        {
            Value = HitCountDown
        });
    }
}


partial class BrickMoveSytem : SystemBase
{
    EntityQuery filldownTaget;
    protected override void OnCreate()
    {
        // Query that contains all of Execute params found in `QueryJob` - as well as additional user specified component `BoidTarget`.
        var desc1 = new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                ComponentType.ReadWrite<Translation>(),
                ComponentType.ReadOnly<FallDownComponent>()
            },
            None = new ComponentType[] {typeof(TweenPositionComponent)}
        };
        filldownTaget = GetEntityQuery(desc1);
    }

    protected override void OnUpdate()
    {
        ref PhysicsWorld world = ref World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;

        NativeList<Entity> fallDownEntites = new NativeList<Entity>(100, Allocator.TempJob);
        NativeList<RaycastHit> raycastHits = new NativeList<RaycastHit>(100, Allocator.TempJob);

        new FallDownRayCastJob
        {
            FallEntities = fallDownEntites,
            RaycastHits = raycastHits,
            CollectAllHits = false,
            World = world,
        }.Schedule(filldownTaget).Complete();

        var length = fallDownEntites.Length;
        for (int i = 0; i < length; i++)
        {
            //var hitPosition = raycastHits[i].Position
            var moveLen = raycastHits[i].Fraction * 5f;
            ITweenComponent.CreateMoveTween(fallDownEntites[i], new float3(0, -moveLen, 0), 0.5f, DG.Tweening.Ease.InCubic, isRelative: true, autoKill: true);
            //EntityManager.RemoveComponent<FallDownComponent>(fallDownEntites[i]); 需要一直检测.
        }
        fallDownEntites.Dispose();
        raycastHits.Dispose();
    }

    private partial struct FallDownRayCastJob : IJobEntity
    {
        //public float Length;
        public NativeList<Unity.Physics.RaycastHit> RaycastHits;
        public bool CollectAllHits; // 没用到
        public NativeList<Entity> FallEntities;
        [ReadOnly] public PhysicsWorld World;

        public void Execute(Entity e, in Translation translation)
        {
            //第二层以上方块
            if (translation.Value.y > 1)
            {
                var maxDistance = 5f;
                var startPos = translation.Value + new float3(0, -0.5f, 0);
                RaycastInput raycastInput = new RaycastInput
                {
                    Start = startPos,
                    End = startPos + math.down() * maxDistance,
                    Filter = CollisionFilter.Default
                };
                if (CollectAllHits)
                {
                    World.CastRay(raycastInput, ref RaycastHits);
                }
                else if (World.CastRay(raycastInput, out Unity.Physics.RaycastHit hit))
                {
                    if (hit.Fraction * maxDistance > 0.1f)//需要下落
                    {
                        FallEntities.Add(e);
                        RaycastHits.Add(hit);
                        //UnityEngine.Debug.Log($"Hit At {hit.Fraction }");
                    }
                }
                //else
                //{
                //    UnityEngine.Debug.Log($"NO Hit {startPos} {raycastInput.End}");
                //}
            }
        }
    }
}

/// <summary>
/// 特殊方块机制
/// </summary>
[UpdateBefore(typeof(HealthEventSystem))]
partial class SpecialBrickSystem : SystemBase
{
    //private NativeParallelMultiHashMap<int, Entity> ChainBrickMap = new NativeParallelMultiHashMap<int, Entity>(8,Allocator.Persistent);
    protected override void OnUpdate()
    {
        //连锁方块1
        NativeParallelMultiHashMap<int, Entity> chainBrickMap = new NativeParallelMultiHashMap<int, Entity>(8, Allocator.TempJob);
        Entities
            .ForEach((Entity e,  ref ChainBrick chainBrick , in DynamicBuffer<HealthEvent> healthEvents) =>
        {
            //加入集合
            chainBrickMap.Add(chainBrick.GroupId, e);
            //判断触发标志
            for (int i = 0; i < healthEvents.Length; i++)
            {
                if (healthEvents[i].state == TRIGGER.RISING_EDGE)
                {
                    chainBrick.IsHited = true;
                }
            }
        }).Schedule();

        Dependency.Complete();

        var keys = chainBrickMap.GetKeyArray(Allocator.Temp);
        for (int i = 0; i < keys.Length; i++)
        {
            var groupBrick = chainBrickMap.GetValuesForKey(keys[i]);
            bool allBreak = true;
            foreach (var item in groupBrick)
            {
                var chainBrick = EntityManager.GetComponentData<ChainBrick>(item);

                if (chainBrick.IsHited == false)
                {
                    allBreak = false;
                    break;
                }
            }
            //消灭所有方块
            if (allBreak)
            {
                foreach (var item in groupBrick)
                {
                    EntityManager.SetComponentData<Health>(item, new Health { Value = 0 });
                }
            }
        }

        chainBrickMap.Dispose();
    }
}
