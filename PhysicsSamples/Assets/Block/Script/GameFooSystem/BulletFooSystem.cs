using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Stateful;
using Unity.Transforms;

public partial class BulletFooSystem : SystemBase
{
    EntityQueryMask _changeElementBrickMask;
    EntityCommandBufferSystem endEcbSys;
    protected override void OnCreate()
    {
        base.OnCreate();
        _changeElementBrickMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                },
                All = new ComponentType[]
                {
                    typeof(HealthElementData), typeof(ChangeElement)
                }
            })
        );
        endEcbSys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = endEcbSys.CreateCommandBuffer();
        //子弹命中砖块后改为砖块对应的元素
        var changeElementBrickMask = _changeElementBrickMask;
        var changeElementBallList = new NativeList<Entity>(Allocator.TempJob);
        var changeViewIndexList = new NativeList<int>(Allocator.TempJob);
        Entities
            .WithAll<BulletComponent>()
            .ForEach((Entity e, ref LifeTime lifetime, in BulletComponent bullet, in DynamicBuffer<StatefulTriggerEvent> triggerEvents) =>
            {
                for (int i = 0; i < triggerEvents.Length; i++)
                {
                    var triggerEvent = triggerEvents[i];
                    var otherEntity = triggerEvent.GetOtherEntity(e);
                    if (triggerEvent.State == StatefulEventState.Enter && HasComponent<BrickRemake>(otherEntity))
                    {
                        lifetime.Value = 30;
                    }
                }
            }).Schedule();

        Entities
            .WithAll<BulletComponent>()
            .ForEach((Entity e, ref Damage damage , in DynamicBuffer<StatefulCollisionEvent> collisionEvents, in ViewChangeAble viewChange) =>
            {
                for (int i = 0; i < collisionEvents.Length; i++)
                {
                    var otherEntity = collisionEvents[i].GetOtherEntity(e);
                    if (changeElementBrickMask.Matches(otherEntity))
                    {
                        var targetElement = GetBuffer<HealthElementData>(otherEntity)[0];
                        damage.DamageElementType = targetElement.elementType;
                        changeElementBallList.Add(e);
                        changeViewIndexList.Add((int)damage.DamageElementType - 1);
                    }
                }
            }).Schedule();
        Dependency.Complete();
        var viewChange = GetComponentDataFromEntity<ViewChangeAble>(true);

        for (int i = 0; i < changeElementBallList.Length; i++)
        {
            var ball = changeElementBallList[i];
            ViewChangeAble.ReplaceChild0(ecb, viewChange[ball], changeViewIndexList[i] , ball, GetBuffer<Child>(ball));
        }
        changeElementBallList.Dispose();
        changeViewIndexList.Dispose();


        //子弹命中合成方块,进入合成方块后停在内部
        Entities
            .ForEach((Entity e, in BulletComponent bullet , in DynamicBuffer<StatefulTriggerEvent> triggerEvents) =>
        {
            for (int i = 0; i < triggerEvents.Length; i++)
            {
                var triggerEvent = triggerEvents[i];
                var otherEntity = triggerEvent.GetOtherEntity(e);

                if (triggerEvent.State == StatefulEventState.Enter
                    && HasComponent<BrickProduct>(otherEntity) && HasComponent<BrickCacheBullet>(otherEntity))
                {
                    var brickProduct = GetComponent<BrickProduct>(otherEntity);
                    if (brickProduct.RecipeBlob.Value.InA.Index == bullet.BulletPerfab
                        || brickProduct.RecipeBlob.Value.InB.Index == bullet.BulletPerfab)
                    {
                        //ecb.Instantiate(brickProduct.RecipeBlob.Value.OutA);
                        ecb.SetComponent(e, GetComponent<Translation>(otherEntity));
                        ecb.RemoveComponent<LifeTime>(e);
                        ecb.SetComponent<PhysicsVelocity>(e, new PhysicsVelocity());
                    }
                    else
                    {
                        ecb.DestroyEntity(e);
                    }
                }
            }
        }).Schedule();
    }
}
