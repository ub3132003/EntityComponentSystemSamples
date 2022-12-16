using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

public struct TweenColorComponent : IComponentData, ITweenComponent
{
    public TweenData Value { get; set; }
}
