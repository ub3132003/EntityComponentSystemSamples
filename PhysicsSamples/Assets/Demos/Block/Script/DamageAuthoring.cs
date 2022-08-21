using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public interface IDamage
{
    public int Value { get; set; }
    public COST_TYPES Type { get; set; }
}
public struct Damage : IComponentData, IDamage
{
    public int Value { get; set; }
    public COST_TYPES Type { get; set; }
}

[DisallowMultipleComponent]
public class DamageAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Value;
    public COST_TYPES Type;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Damage
        {
            Value = Value,
            Type = Type,
        });
    }
}
