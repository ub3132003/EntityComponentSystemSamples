using System;
using Unity.Entities;

[Serializable]
public struct SteerForFear : IComponentData
{
    public float Weight;
    public float WeighedForce;
}
