using DG.Tweening;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public abstract class TweenAuthoring<T> : MonoBehaviour, IConvertGameObjectToEntity where T : struct, IComponentData, ITweenComponent
{
    public float Lifetime = 1f;
    public Ease ease = Ease.InQuad;
    public bool isReset;
    public bool isRelative;
    public bool AutoKill = true;
    public bool isLoop;
    public LoopMode loopMode;
    public float3 From;
    public float3 To;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TweenData
        {
            Start = new float4(transform.position, 0),
            PassTime = -1,
            Duration = Lifetime,
            ease = ease,
            isReset = isReset,
            isRelative = isRelative,
            AutoKill = AutoKill,
            isLoop = isLoop,
            loopMode = loopMode,
            From = new float4(From, 0f),
            To = new float4(To, 0f),
        };
        dstManager.AddComponentData(entity, CreateComponent(data));
    }

    protected abstract T CreateComponent(TweenData tweenData);
}
