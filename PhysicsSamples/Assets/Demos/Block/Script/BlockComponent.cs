using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable][GenerateAuthoringComponent]
public struct BlockComponent : IComponentData
{
    //倒数命中计数，0时爆碎
    public int HitCountDown;
}
