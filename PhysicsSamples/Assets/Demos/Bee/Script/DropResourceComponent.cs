using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct DropResourceComponent : IComponentData
{
    public float3 position;
    public bool stacked;
    public int stackIndex;
    public Entity holder;
    public float3 velocity;
    public bool dead;
}
