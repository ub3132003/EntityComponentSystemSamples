using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Stateful;
using Unity.Collections;
using System;

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
    protected override void OnCreate()
    {
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

                    switch (damage.Type)
                    {
                        case COST_TYPES.FLAT:
                            health.Value -= damage.Value;
                            break;
                        case COST_TYPES.PERCENT_OF_MAX:
                            break;
                        case COST_TYPES.PERCENT_OF_CURRENT:
                            health.Value -= (int)math.ceil(health.Value * (damage.Value / 100f));
                            break;
                        default:
                            break;
                    }
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

                //死亡
                if (health.Value == 0)
                {
                    ecb.DestroyEntity(e);

                    break;
                }
                else if (health.Value < 0)
                {
                    ecb.DestroyEntity(e);

                    break;
                }
                else
                {
                    //tweenTarget.Add(blockEntity);
                }
            }
        }).Schedule();


        destorySys.AddJobHandleForProducer(this.Dependency);
    }
}
