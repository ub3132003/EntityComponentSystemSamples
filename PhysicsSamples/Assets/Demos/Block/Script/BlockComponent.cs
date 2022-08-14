using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable][GenerateAuthoringComponent]
public struct BlockComponent : IComponentData
{
    //倒数命中计数，0时爆碎
    public int HitCountDown;

    /// <summary>
    /// 死亡时掉落金币数量
    /// </summary>
    public int DieDropCount;
}
