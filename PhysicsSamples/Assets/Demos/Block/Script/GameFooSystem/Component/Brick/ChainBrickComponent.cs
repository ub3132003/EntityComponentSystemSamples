using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct ChainBrick : IComponentData
{
    //所属集合的Id
    public int GroupId;
    public bool IsHited;
}
