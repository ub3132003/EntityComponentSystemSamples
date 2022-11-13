using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 从ecs 取得通讯
/// </summary>
public class PlayerSharedData : ISharedComponentData
{
    /// <summary>
    /// 经验值
    /// </summary>
    public int Exp;
}
