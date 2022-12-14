using DG.Tweening;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct TweenData
{
    public TweenData(Entity target, float4 to, float lifetime)
    {
        From = default;
        TweenEntity = target;
        To = to;
        Duration = lifetime;
        PassTime = 0;
        ease = Ease.OutQuad;
        isReset = false;
        Start = new float4();
        End = default;
        isRelative = false;
        AutoKill = true;

        isLoop = false;
        loopMode = default;
    }

    /// <summary>
    /// 已经过去的时间
    /// </summary>
    public float PassTime { get; set; }
    public Entity TweenEntity { get; set; }

    public float4 Start { get; set; }
    public float4 End { get; set; }
    public float Duration { get; set; }
    public Ease ease { get; set; }
    public bool isReset { get; set; }//完成时重置到for
    public bool isRelative { get; set; }
    public bool AutoKill { get; set; }
    public bool isLoop { get; set; }
    public LoopMode loopMode { get; set; }
    public float4 From { get; set; }
    public float4 To { get; set; }

    public bool IsComplete => PassTime > Duration;


    public void SetDelay(Entity tweenTarget, float delay)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var tweener = entityManager.GetComponentData<TweenPositionComponent>(tweenTarget);
        tweener.PassTime -= delay;
        entityManager.SetComponentData(tweenTarget, tweener);
    }
}
