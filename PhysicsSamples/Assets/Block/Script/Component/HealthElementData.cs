using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 元素类型生命值,无法被一般伤害扣除.
/// </summary>
[Serializable]
[GenerateAuthoringComponent]
public struct HealthElementData : IBufferElementData
{
    public ElementType elementType;
    public float Value;
}
