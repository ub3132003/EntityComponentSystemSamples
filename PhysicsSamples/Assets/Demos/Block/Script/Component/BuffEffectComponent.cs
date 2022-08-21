using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;

[InternalBufferCapacity(8)]
public struct BuffEffectComponent : IBufferElementData
{
    public int StackLimit { get; }
    //public bool AllowMultiple, AllowMixedCaster;
    public int Pulses { get; }
    public float Duration { get; }
    public bool Endless { get; }

    public float PulseInterval => Duration / Pulses;

    public BuffEffectComponent(int stackLimit, int pulses, float duration, bool endless)
    {
        StackLimit = stackLimit;
        Pulses = pulses;
        Duration = duration;
        Endless = endless;
    }
}
partial class BuffEffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .ForEach((ref DynamicBuffer<BuffEffectComponent> buffs , ref TimeDataComponent time) =>
        {
            var length = buffs.Length;
            for (int i = 0; i < length; i++)
            {
                var buff = buffs[i];
                if (buff.Endless)
                {
                }
                else
                {
                    if (buff.Duration > time.Value.ElapsedTime)
                    {
                        buffs.RemoveAt(i);
                        continue;
                    }
                }
            }
        }).Schedule();
    }
}
