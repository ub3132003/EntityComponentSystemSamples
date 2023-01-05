using System;
using System.Collections;
using Unity.Entities;
using UnityEngine;
/// <summary>
/// 实体事件，用于和mono 通讯
/// </summary>
public struct EntityEventComponent : ISharedComponentData, IEquatable<EntityEventComponent>
{
    public EntityChannelSO entityEvent;

    public bool Equals(EntityEventComponent other)
    {
        return other.entityEvent == entityEvent;
    }

    public override int GetHashCode()
    {
        return entityEvent.GetHashCode();
    }
}

public class EntityEventSend : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] EntityChannelSO createEvent;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddSharedComponentData(entity, new EntityEventComponent
        {
            entityEvent = createEvent
        });
    }
}
