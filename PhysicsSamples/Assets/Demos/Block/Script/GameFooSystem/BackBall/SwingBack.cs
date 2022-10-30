using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
/// <summary>
/// 玩家点击大打回球
/// </summary>
[GenerateAuthoringComponent]
public struct SwingBack : IComponentData
{
    /// <summary>
    /// 可回击冷却时间
    /// </summary>
    public float Rate;
}
