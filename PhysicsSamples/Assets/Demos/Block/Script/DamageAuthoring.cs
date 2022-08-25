using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public interface IDamage
{
    public int DamageValue { get; set; }
    public COST_TYPES Type { get; set; }
}
public struct Damage : IComponentData, IDamage
{
    public int DamageValue { get; set; }
    public COST_TYPES Type { get; set; }
    public Health DealHealth(Health health)
    {
        switch (Type)
        {
            case COST_TYPES.FLAT:
                health.Value -= DamageValue;
                break;
            case COST_TYPES.PERCENT_OF_MAX:
                break;
            case COST_TYPES.PERCENT_OF_CURRENT:
                health.Value -= (int)math.ceil(health.Value * (DamageValue / 100f));
                break;
            default:
                break;
        }
        return health;
    }
}

[DisallowMultipleComponent]
public class DamageAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Min(1)]
    public int Value;
    public COST_TYPES Type;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new Damage
        {
            DamageValue = Value,
            Type = Type,
        });
    }
}
