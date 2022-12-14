using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[GenerateAuthoringComponent]
public struct TweenColorComponent : IComponentData
{
    public TweenData Vaule;
}
