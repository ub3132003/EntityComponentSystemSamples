using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Physics.Stateful;
using Unity.Collections;

public struct Health : IComponentData
{
    public int Value;
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

        Entities
            .ForEach((Entity e , ref DynamicBuffer<StatefulCollisionEvent> collisonEvents, ref Health health) =>
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
                    health.Value -= damage.Value;
                    //Debug.Log($"H:{health.Value} D:{damage.Value}");
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

        //}).Schedule();
        destorySys.AddJobHandleForProducer(this.Dependency);
        //deadEntities.Dispose();
    }
}
