using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Abillity : IComponentData
{
    public Entity Caster;
}
