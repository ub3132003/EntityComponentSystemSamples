using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Stateful;
using Unity.Collections;
using System;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Systems;

public struct Health : IComponentData, IComparable<Health>
{
    public int Value;

    public int CompareTo(Health other)
    {
        return Value.CompareTo(other.Value);
    }
}


[DisallowMultipleComponent]
public class HealthAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Health;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Health
        {
            Value = Health
        });
    }
}
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
partial class HealthSystem : SystemBase
{
    EntityQueryMask damagerCollionMask;
    private BuildPhysicsWorld m_BuildPhysicsWorld;
    protected override void OnCreate()
    {
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();

        damagerCollionMask = EntityManager.GetEntityQueryMask(
            GetEntityQuery(new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(StatefulTriggerEvent)
                },
                All = new ComponentType[]
                {
                    typeof(Damage)
                }
            })
        );
    }

    protected override void OnUpdate()
    {
        //NativeList<Entity> deadEntities = new NativeList<Entity>(10, Allocator.TempJob);
        EntityCommandBufferSystem destorySys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        var ecb = destorySys.CreateCommandBuffer();
        var damageMask = damagerCollionMask;

        var eventBuffers = GetBufferFromEntity<HealthEvent>();

        //碰撞触发的伤害 投射技能
        Entities
            .ForEach((Entity e , ref DynamicBuffer<StatefulCollisionEvent> collisonEvents, ref Health health , ref DynamicBuffer<BuffEffectComponent> buffEffects) =>
        {
            var length = collisonEvents.Length;
            for (int i = 0; i < length; i++)
            {
                var collisonEvent = collisonEvents[i];
                var damageEntity = collisonEvent.GetOtherEntity(e);
                //Debug.Log($"CS:{collisonEvent.State} D:{damageMask.Matches(damageEntity)}");
                if (collisonEvent.State != StatefulEventState.Enter || !damageMask.Matches(damageEntity))
                {
                    continue;
                }


                if (HasComponent<Damage>(damageEntity))
                {
                    var damage = GetComponent<Damage>(damageEntity);
                    health = damage.DealHealth(health);

                    //Debug.Log($"H:{health.Value} D:{damage.Value}");
                }
                if (HasComponent<DamagetOverTime>(damageEntity))
                {
                    var damagetOverTime = GetComponent<DamagetOverTime>(damageEntity);


                    var newbuff = new BuffEffectComponent
                    {
                        buffRef = damagetOverTime.buffRef,
                        stateCurDuration = 0,
                        stateMaxDuration = damagetOverTime.buffRef.Value.Duration,
                        curStack = 1,
                        maxStack = damagetOverTime.buffRef.Value.StackLimit,
                    };
                    //TODO 堆叠
                    var buffIndex = buffEffects.FindBuffIndex(newbuff);
                    //没找到时新增，找到时替换
                    if (buffIndex < 0)
                    {
                        buffEffects.Add(newbuff);
                    }
                    else
                    {
                        var oldBuff = buffEffects[buffIndex];

                        newbuff.curStack = oldBuff.curStack < oldBuff.maxStack ? oldBuff.curStack + 1 : oldBuff.curStack;


                        buffEffects[buffIndex] = newbuff;
                    }


                    Debug.Log($"Add Buff e:{e} b:{newbuff.buffRef} ");
                }

                //更新事件比较 TODO
                if (eventBuffers.HasComponent(e))
                {
                    var eventbuffer = eventBuffers[e];
                    //eventBuffers[e].Clear();
                    for (int j = 0; j < eventbuffer.Length; j++)
                    {
                        eventbuffer[j] = HealthEvent.CompareEvent(eventbuffer[j], health.Value);
                    }
                }
            }
        }).Schedule();


        var physicsWorld = m_BuildPhysicsWorld.PhysicsWorld;
        //主动触发的伤害Aoe ,通过overlay
        Entities
            .WithAll<Abillity>()
            .ForEach((Entity e, in ExplodeComponent explode, in Damage damage, in Translation t, in Rotation r) =>
            {
                var distanceHits = new NativeList<DistanceHit>(8, Allocator.Temp);

                if (physicsWorld.CollisionWorld.OverlapSphere(t.Value, explode.ExplodeHalfRange, ref distanceHits, explode.Filter))
                {
                    //Debug.Log($"hit: {distanceHits[0].Entity}");
                    for (int j = 0; j < distanceHits.Length; j++)
                    {
                        var other = distanceHits[j].Entity;

                        if (HasComponent<Health>(other))
                        {
                            //TODO 解耦伤害计算?
                            var hp = GetComponent<Health>(other);
                            if (hp.Value > 0)
                            {
                                SetComponent(other, damage.DealHealth(hp));
                            }
                        }
                    }
                }
                ecb.RemoveComponent<ExplodeComponent>(e);
            }).Schedule();

        //对象立即伤害
        Entities
            .ForEach((in Abillity ab, in SelfComponent self, in Damage damage) =>
        {
            if (HasComponent<Health>(ab.Caster))
            {
                var hp = GetComponent<Health>(ab.Caster);
                hp = damage.DealHealth(hp);
                SetComponent(ab.Caster, hp);
            }
        }).Schedule();


        Entities
            .ForEach((Entity e, in Health health) =>
        {
            //死亡
            if (health.Value == 0)
            {
                ecb.DestroyEntity(e);
            }
            else if (health.Value < 0)
            {
                ecb.DestroyEntity(e);
            }
            else
            {
                //tweenTarget.Add(blockEntity);
            }
        }).Schedule();

        destorySys.AddJobHandleForProducer(this.Dependency);
    }
}
