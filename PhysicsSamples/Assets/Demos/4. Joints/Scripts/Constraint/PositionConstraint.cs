using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Transforms;
public struct PositionConstraintComponent : IComponentData
{
    public float3 PositionOffset;
    public bool3 FreezePositionAxes;
    public Entity Sources;
}

[DisallowMultipleComponent]
public class PositionConstraint : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 PositionOffset;

    public bool3 FreezePositionAxes;

    public GameObject Sources;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PositionConstraintComponent
        {
            PositionOffset = PositionOffset,
            FreezePositionAxes = FreezePositionAxes,
        });
    }
}

partial class PositionConstraintSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithName("PositionConstraint")
            .WithAll<Translation>()
            .WithoutBurst()
            .ForEach((Entity entity, ref Translation t , in PositionConstraint pc) => {
            }).Run();
    }
}
