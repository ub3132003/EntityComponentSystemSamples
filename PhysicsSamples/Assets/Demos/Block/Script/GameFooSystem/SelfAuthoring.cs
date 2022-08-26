using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public struct SelfComponent : IComponentData
{
}

[DisallowMultipleComponent]
public class SelfAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SelfComponent());
    }
}
