using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Systems;
using Unity.Physics.Stateful;
using Unity.Rendering;

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
            None = new ComponentType[] { typeof(TweenPositionComponent) }
        };
        filldownTaget = GetEntityQuery(desc1);
    }

    protected override void OnUpdate()
    {
        ref PhysicsWorld world = ref World.DefaultGameObjectInjectionWorld.GetExistingSystem<BuildPhysicsWorld>().PhysicsWorld;
        EntityCommandBufferSystem sys = this.World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = sys.CreateCommandBuffer();

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
            var tween = new TweenData(TypeOfTween.Position, fallDownEntites[i], new float4(0, -moveLen, 0, 0), .5f)
                .SetEase(DG.Tweening.Ease.InCubic)
                .SetIsRelative(true);
            TweenCreateSystem.AddTweenComponent<TweenPositionComponent>(ecb, tween);
            //ITweenComponent.CreateMoveTween(fallDownEntites[i], new float3(0, -moveLen, 0), 0.5f, DG.Tweening.Ease.InCubic, isRelative: true, autoKill: true);
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

        public void Execute(Entity e, in Translation translation , in BrickComponent brickComponent)
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
    EntityQueryMask elementBoxMask;
    EntityCommandBufferSystem sysEnd;
    protected override void OnCreate()
    {
        base.OnCreate();

        elementBoxMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                },
                All = new ComponentType[]
                {
                    typeof(Health), typeof(HealthElementData), typeof(BrickComponent)
                }
            })
        );
        sysEnd = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //最后一行砖块z位置
    //int lastBrickZ = -9;
    int frameIdx = 0;
    protected override void OnUpdate()
    {
        EntityManager em = World.DefaultGameObjectInjectionWorld.EntityManager;
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        EntityCommandBuffer ecbSysEnd = sysEnd.CreateCommandBuffer();
        //连锁方块1
        NativeParallelMultiHashMap<int, Entity> chainBrickMap = new NativeParallelMultiHashMap<int, Entity>(8, Allocator.TempJob);
        Entities
            .ForEach((Entity e, ref ChainBrick chainBrick, in DynamicBuffer<HealthEvent> healthEvents) =>
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


        //看向挡板
        var playerEntity = PlayerEcsConnect.Instance.Player;
        Entities
            .ForEach((ref Rotation r, in Translation t, in EyeBrickTag eye) =>
        {
            var playTrans = GetComponent<Translation>(playerEntity);
            var dir = t.Value - playTrans.Value;
            r.Value = quaternion.LookRotation(dir, math.up());
        }).Schedule();

        //第一层方块移动允许移动
        var delteTime = Time.DeltaTime;
        Entities
            .ForEach((ref PhysicsVelocity pv, in BrickMoveComponent brickMove) =>
        {
            if (pv.Linear.y <= 1)
            {
                pv.Linear = brickMove.Velocity;
            }
        }).Schedule();


        frameIdx++;

        //元素方块
        var _elementBoxMask = this.elementBoxMask;
        //命中时添加元素buff
        Entities
            .ForEach((Entity e, in Damage damage, in DynamicBuffer<StatefulCollisionEvent> collisionEvents) =>
        {
            var length = collisionEvents.Length;
            for (int i = 0; i < length; i++)
            {
                var collisonEvent = collisionEvents[i];
                var otherEntity = collisonEvent.GetOtherEntity(e);
                if (collisonEvent.State != StatefulEventState.Enter || !_elementBoxMask.Matches(otherEntity))
                {
                    continue;
                }
                var health = GetComponent<Health>(otherEntity);
                if (health.Value <= 0) continue;
                var healthElement = GetBuffer<HealthElementData>(otherEntity);
                if (damage.DamageElementType != ElementType.NONE && damage.DamageElementType != ElementType.EARTH)
                {
                    healthElement.Add(new HealthElementData { elementType = damage.DamageElementType });
                }
            }
        }).Schedule();
        //元素buff 反应 处理
        NativeList<Entity> changeMatColorList = new NativeList<Entity>(Allocator.TempJob);
        NativeList<int> colorList = new NativeList<int>(Allocator.TempJob);
        Entities
            .ForEach((Entity e, ref DynamicBuffer<HealthElementData> healthElementDatas , in DynamicBuffer<Child> childs) =>
        {
            var length = healthElementDatas.Length;
            if (length < 2) return;
            var firstElement = healthElementDatas[0];
            var secondElement = healthElementDatas[1];
            switch (firstElement.elementType)
            {
                case ElementType.NONE:
                    break;
                case ElementType.EARTH:

                    firstElement.elementType = secondElement.elementType;
                    changeMatColorList.Add(e);
                    //changeMatColorList.Add(childs[0].Value);
                    colorList.Add((int)firstElement.elementType);
                    break;
                case ElementType.WATER:
                    break;
                case ElementType.ICE:
                    break;
                case ElementType.ELECTRIC:
                    break;
                case ElementType.FIRE:
                    break;
            }
            healthElementDatas[0] = firstElement;
            healthElementDatas.RemoveAt(1);
        }).Schedule();
        Dependency.Complete();
        //改颜色
        var length = changeMatColorList.Length;
        //for (int i = 0; i < length; i++)
        //{
        //    var e = changeMatColorList[i];
        //    if (HasComponent<URPMaterialPropertyBaseColor>(e))
        //    {
        //        SetComponent(e, new URPMaterialPropertyBaseColor
        //        {
        //            Value = colorList[i]
        //        });
        //    }
        //}
        //改实体
        ComponentDataFromEntity<Parent> parentFromEntity = GetComponentDataFromEntity<Parent>(true);
        ComponentDataFromEntity<LocalToParent> localToParentFromEntity = GetComponentDataFromEntity<LocalToParent>(true);
        BufferFromEntity<Child> linkedEntityBufferFromEntity = GetBufferFromEntity<Child>(false);
        Job.WithCode(() =>
        {
            for (int i = 0; i < length; i++)
            {
                var e = changeMatColorList[i];
                if (HasComponent<ViewChangeAble>(e))
                {
                    var viewData = GetComponent<ViewChangeAble>(e);
                    var viewPrefab = viewData.ViewPrefabBlob.Value.PrefabArray[colorList[i]];
                    var newEntity = ecb.Instantiate(e);
                    var newView = ecb.Instantiate(viewPrefab);
                    EntityHelp.SetParent(ecb, newEntity, newView, float3.zero, quaternion.identity);
                    ecbSysEnd.DestroyEntity(e);
                    //childs.Add(new Child { Value = newView });
                }
            }
        }).Run();
        Dependency.Complete();//? 为什么要等待这个任务?
        ecb.Playback(this.EntityManager);
        ecb.Dispose();
        //sysEnd.AddJobHandleForProducer(Dependency);
        changeMatColorList.Dispose();
        colorList.Dispose();
    }

    public void DealElement(HealthElementData element1, HealthElementData element2)
    {
    }

    public static float4 ChangeElementColor(ElementType elementType)
    {
        var outColor = float4.zero;
        switch (elementType)
        {
            case ElementType.NONE:
                break;
            case ElementType.EARTH:
                break;
            case ElementType.WATER:
                outColor = UnityEngine.Color.blue.ToFloat4();
                break;
            case ElementType.ICE:
                outColor = UnityEngine.Color.cyan.ToFloat4();
                break;
            case ElementType.ELECTRIC:
                outColor = new float4(148, 0, 211, 255) / 255f;
                break;
            case ElementType.FIRE:
                outColor = UnityEngine.Color.red.ToFloat4();
                break;
            default:
                break;
        }
        return outColor;
    }
}
