using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct SteerForPursuit : IComponentData
{
    //׷���Ŀ��
    public Entity Quarry;
    public float Weight;
    public float MaxPredictionTime;
    //�������룿
    public float AcceptableDistance;
    public bool _slowDownOnApproach;
    public float3 WeighedForce;
}
