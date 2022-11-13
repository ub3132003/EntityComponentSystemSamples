using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;


public struct BuffEffectComponent : IBufferElementData, IEquatable<BuffEffectComponent>
{
    public int StackLimit { get; }
    //public bool AllowMultiple, AllowMixedCaster;
    public int Pulses { get; }
    public float Duration { get; }
    public bool Endless { get; }

    public float PulseInterval => Duration / Pulses;

    public BlobAssetReference<BuffBlobAsset> buffRef;

    public int maxPulses;
    public int curPulses;
    public float nextPulse;
    public float pulseInterval;
    public float stateMaxDuration;
    public float stateCurDuration;
    public int curStack;
    public int maxStack;

    //同种buff
    public bool Equals(BuffEffectComponent other)
    {
        return other.buffRef == buffRef;
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        return obj is BuffEffectComponent && Equals((BuffEffectComponent)obj);
    }

    public override int GetHashCode()
    {
        return this.GetHashCode();
    }

    public static bool operator==(BuffEffectComponent left, BuffEffectComponent right)
    {
        return left.Equals(right);
    }

    public static bool operator!=(BuffEffectComponent left, BuffEffectComponent right)
    {
        return !left.Equals(right);
    }
}

public class BuffEffectBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddBuffer<BuffEffectComponent>(entity);
    }
}
partial class BuffEffectSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        Entities
            .ForEach((ref DynamicBuffer<BuffEffectComponent> buffs) =>
        {
            var length = buffs.Length;
            NativeList<int> removeIdx = new NativeList<int>(length, Allocator.Temp);
            for (int i = 0; i < length; i++)
            {
                var buff = buffs[i];
                buff.stateCurDuration += deltaTime;

                if (buff.curPulses > 0)
                {
                    buff.nextPulse -= deltaTime;
                }
                if (buff.nextPulse <= 0 && buff.curPulses < buff.maxPulses)
                {
                    buff.nextPulse = buff.pulseInterval;
                    buff.curPulses++;
                }
                buffs[i] = buff;
                if (buff.stateCurDuration > buff.stateMaxDuration)
                {
                    removeIdx.Add(i);
                }
            }
            length = removeIdx.Length;
            // end buff
            for (int i = 0; i < length; i++)
            {
                HandleEffectEnd(buffs, removeIdx[i]);
            }
        }).Schedule();
    }

    static void HandleEffectEnd(DynamicBuffer<BuffEffectComponent> buffs, int nodeStateIndex)
    {
        Debug.Log($"End Buff{buffs[nodeStateIndex].buffRef}");
        buffs.RemoveAt(nodeStateIndex);
    }
}
