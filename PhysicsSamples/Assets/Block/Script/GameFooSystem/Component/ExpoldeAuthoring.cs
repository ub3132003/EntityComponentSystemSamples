using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using UnityEngine;
using Unity.Physics.Authoring;

public struct ExplodeComponent : IDamage, IComponentData
{
    public float ExplodeHalfRange;

    public CollisionFilter Filter;

    public int DamageValue { get; set; }
    public COST_TYPES Type { get; set; }
}
[DisallowMultipleComponent]
public class ExpoldeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float ExplodeHalfRange;

    public int Value;
    public COST_TYPES Type;
    public PhysicsCategoryTags BelongsTo = new PhysicsCategoryTags { Category00 = true }; // You can switch / add / remove the default "layers" here.
    public PhysicsCategoryTags CollidesWith = new PhysicsCategoryTags { Category01 = true };
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ExplodeComponent
        {
            DamageValue = Value,
            Type = Type,
            ExplodeHalfRange = ExplodeHalfRange,
            Filter = new CollisionFilter
            {
                BelongsTo = BelongsTo.Value,
                CollidesWith = CollidesWith.Value
            }
        });
    }
}
