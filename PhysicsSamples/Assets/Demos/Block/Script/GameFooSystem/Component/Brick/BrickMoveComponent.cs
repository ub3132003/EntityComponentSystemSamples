using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 第一次 方块 移动
/// </summary>
///
[GenerateAuthoringComponent]
public struct BrickMoveComponent : IComponentData
{
    public float3 Velocity;
}
