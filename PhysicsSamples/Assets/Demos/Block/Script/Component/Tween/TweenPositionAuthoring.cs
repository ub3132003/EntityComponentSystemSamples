using DG.Tweening;
using Unity.Entities;
using Unity.Mathematics;


public struct TweenPositionComponent : IComponentData, ITweenComponent
{
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
}

class TweenPositionAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public float  Lifetime;
    public Ease   ease;
    public bool   isReset;
    public bool   isRelative;
    public bool   AutoKill;
    public bool   isLoop;
    public LoopMode loopMode;
    public float3 From;
    public float3 To;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new TweenPositionComponent
        {
            Start = new float4(transform.position.x, transform.position.y, transform.position.z, 0),

            Lifetime = Lifetime,
            ease = ease,
            isReset = isReset,
            isRelative = isRelative,
            AutoKill = AutoKill,
            isLoop = isLoop,
            loopMode = loopMode,
            From = From,
            To = To,
        });
    }
}
