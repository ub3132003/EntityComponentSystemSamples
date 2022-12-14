using DG.Tweening;
using System.Net.Sockets;
using Unity.Entities;
using Unity.Mathematics;


public struct TweenPositionComponent : IComponentData, ITweenComponent
{
    public TweenData Value;
    /// <summary>
    /// 已经过去的时间
    /// </summary>
    public float PassTime { get; set; }
    public Entity TweenEntity { get; set; }

    public float4 Start { get; set; }
    public float4 End { get; set; }
    public float Lifetime { get; set; }
    public Ease ease { get; set; }
    public bool isReset { get; set; }//完成时重置到for
    public bool isRelative { get; set; }
    public bool AutoKill { get; set; }
    public bool isLoop { get; set; }
    public LoopMode loopMode { get; set; }
    public float3 From { get; set; }
    public float3 To { get; set; }

    public bool IsComplete => PassTime > Lifetime;


    public void SetDelay(Entity tweenTarget, float delay)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var tweener = entityManager.GetComponentData<TweenPositionComponent>(tweenTarget);
        tweener.PassTime -= delay;
        entityManager.SetComponentData(tweenTarget, tweener);
    }

    public void SetToValue(float3 to)
    {
        To = to;
    }

    public void SetToValue(float4 to)
    {
        throw new System.NotImplementedException();
    }
}

class TweenPositionAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public float  Lifetime = 1f;
    public Ease   ease;
    public bool   isReset;
    public bool   isRelative;
    public bool   AutoKill = true;
    public bool   isLoop;
    public LoopMode loopMode;
    public float3 From;
    public float3 To;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var data = new TweenData
        {
            Start = new float4(transform.position, 0),

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
        dstManager.AddComponentData(entity, new TweenPositionComponent
        {
            Value = data
        });
    }
}
