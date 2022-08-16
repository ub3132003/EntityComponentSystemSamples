using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TriggerVolumeVelocityChange : IComponentData
{
    /// <summary>
    /// 增加速度 箭头方向
    /// </summary>
    public float3 AppleVelocity;
    public float MaxSpeed;
}

public struct TriggerVolumeSpeedChange : IComponentData
{
    /// <summary>
    ///  加减速度，保持方向不变
    /// </summary>

    public float Speed;
}
