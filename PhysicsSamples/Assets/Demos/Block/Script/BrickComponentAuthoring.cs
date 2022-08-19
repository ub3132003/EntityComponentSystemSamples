using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Transforms;

[Serializable]
public struct BlockComponent : IComponentData
{
    //倒数命中计数，0时爆碎
    public int HitCountDown;

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
        dstManager.AddComponentData(entity, new BlockComponent
        {
            HitCountDown = HitCountDown,

            DieDropCount = DieDropCount,
        });
        dstManager.AddComponentData(entity, new FallDownComponent());
    }
}
partial class BrickMoveSytem : SystemBase
{
    EntityQuery filldownTaget;
    protected override void OnCreate()
    {
        // Query that contains all of Execute params found in `QueryJob` - as well as additional user specified component `BoidTarget`.
        filldownTaget = GetEntityQuery(ComponentType.ReadWrite<Translation>(), ComponentType.ReadOnly<FallDownComponent>());
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
            ITweenComponent.CreateMoveTween(fallDownEntites[i], new float3(0, -moveLen, 0), 0.5f, DG.Tweening.Ease.InCubic, isRelative: true);
            EntityManager.RemoveComponent<FallDownComponent>(fallDownEntites[i]);
        }
        fallDownEntites.Dispose();
        raycastHits.Dispose();
    }

    private partial struct FallDownRayCastJob : IJobEntity
    {
        //public float Length;
        public NativeList<RaycastHit> RaycastHits;
        public bool CollectAllHits; // 没用到
        public NativeList<Entity> FallEntities;
        [ReadOnly] public PhysicsWorld World;

        public void Execute(Entity e, in Translation translation)
        {
            var maxDistance = 5f;
            var startPos = translation.Value + new float3(0, -0.5f, 0);
            RaycastInput raycastInput = new RaycastInput
            {
                Start = startPos,
                End = startPos + math.down()  * maxDistance,
                Filter = CollisionFilter.Default
            };
            if (CollectAllHits)
            {
                World.CastRay(raycastInput, ref RaycastHits);
            }
            else if (World.CastRay(raycastInput, out RaycastHit hit))
            {
                RaycastHits.Add(hit);
                if (hit.Fraction * maxDistance > 0.1f)//需要下落
                {
                    FallEntities.Add(e);
                }
            }
        }
    }
}
