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
        var constact = new PositionConstraintComponent
        {
            PositionOffset = PositionOffset,
            FreezePositionAxes = FreezePositionAxes,
            Sources = conversionSystem.GetPrimaryEntity(Sources),
        };
        if (constact.Sources == Entity.Null)
        {
            Debug.LogWarning("No Entity Find");
            return;
        }
        dstManager.AddComponentData(entity, constact);
    }
}

partial class PositionConstraintSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        var ecb = sys.CreateCommandBuffer();

        Entities
            .WithName("PositionConstraint")
            .WithAll<Translation>()
            .WithoutBurst()
            .ForEach((Entity entity, in Translation t, in PositionConstraintComponent pc) => {
                var targetPosition = GetComponent<LocalToWorld>(pc.Sources).Position + pc.PositionOffset;
                ecb.SetComponent<Translation>(entity, new Translation
                {
                    Value = targetPosition
                });
            }).Run();

        sys.AddJobHandleForProducer(this.Dependency);
    }
}
