using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
[GenerateAuthoringComponent]
public struct TopDownPlayer : IComponentData
{
    public Entity ControlledCharacter;
    [NonSerialized]
    public uint LastInputsProcessingTick;
}
