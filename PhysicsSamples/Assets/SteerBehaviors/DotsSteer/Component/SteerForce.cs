using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SteerForce : IComponentData
{
    public float3 Force;

    public float3 PassForce;
}
