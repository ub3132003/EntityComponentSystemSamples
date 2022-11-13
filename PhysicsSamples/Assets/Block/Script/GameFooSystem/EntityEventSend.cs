using System.Collections;
using Unity.Entities;
using UnityEngine;

public sealed class EntityEventComponent : IComponentData
{
    public EntityChannelSO entityEvent;
}

public class EntityEventSend : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] EntityChannelSO createEvent;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new EntityEventComponent
        {
            entityEvent = createEvent
        });
    }
}
partial class EntityEventSystem : SystemBase
{
    EntityQuery m_Query;

    protected override void OnCreate()
    {
        var queryDesc = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(EntityEventComponent)}
        };

        m_Query = GetEntityQuery(queryDesc);
    }

    protected override void OnUpdate()
    {
        if (m_Query.CalculateEntityCount() == 0)
        {
            return;
        }
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;
        Entities
            .WithStructuralChanges()
            .WithoutBurst()
            .ForEach((Entity e, EntityEventComponent entityEvent) =>
            {
                entityEvent.entityEvent.RaiseEvent(e);
                em.RemoveComponent<EntityEventComponent>(e);
            }).Run();
    }
}
