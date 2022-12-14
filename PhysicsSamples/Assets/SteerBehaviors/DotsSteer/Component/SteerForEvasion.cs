using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SteerForEvasion : IComponentData
{
    public float SafetyDistance;

    public float _predictionTime;

    public float3 WeighedForce;
    public float Weighe;
    //Todo嗣醴梓枅燭
    public Entity Menace;
}
