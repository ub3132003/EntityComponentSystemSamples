using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SteerForPursuit : IComponentData
{
    //×·ÖðµÄÄ¿±ê
    public Entity Quarry;
    public float Weight;
    public float MaxPredictionTime;
    //¹¥»÷¾àÀë£¿
    public float AcceptableDistance;
    public bool _slowDownOnApproach;
    public float3 WeighedForce;
}
