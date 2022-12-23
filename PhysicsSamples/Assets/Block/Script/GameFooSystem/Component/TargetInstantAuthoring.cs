using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public struct TargetInstant : IComponentData
{
    public Entity Caster;
    public Entity Target;
}

[DisallowMultipleComponent]
public class TargetInstantAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new TargetInstant());
    }
}
