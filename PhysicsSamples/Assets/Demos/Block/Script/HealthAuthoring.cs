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

partial class HealthSystem : SystemBase
{
    protected override void OnUpdate()
    {
        NativeList<Entity> deadEntities = new NativeList<Entity>(10, Allocator.TempJob);
        Entities
            .ForEach((Entity e , ref DynamicBuffer<StatefulCollisionEvent> collisonEvents, ref Health health) =>
        {
            var length = collisonEvents.Length;
            for (int i = 0; i < length; i++)
            {
                var collisonEvent = collisonEvents[i];
                var damageEntity = collisonEvent.GetOtherEntity(e);

                if (HasComponent<Damage>(damageEntity))
                {
                    var damage = GetComponent<Damage>(damageEntity);
                    health.Value -= damage.Value;
                }
            }
        }).Schedule();

        EntityCommandBufferSystem destorySys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        var ecb = destorySys.CreateCommandBuffer();

        Entities
            .ForEach((Entity e, in Health health) =>
        {
            if (health.Value == 0)
            {
                deadEntities.Add(e);
                ecb.DestroyEntity(e);
            }
            else if (health.Value < 0)
            {
                deadEntities.Add(e);
            }
            else
            {
                //tweenTarget.Add(blockEntity);
            }
        }).Schedule();

        deadEntities.Dispose();
    }
}
