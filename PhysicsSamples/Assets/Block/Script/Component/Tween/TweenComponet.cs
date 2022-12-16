using DG.Tweening;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
public enum TypeOfTween
{
    None,
    Position,
    Color,
    HdrColor,
}
[Serializable]
public struct TweenData
{
    public TweenData(TypeOfTween type , Entity target, float4 to, float lifetime)
    {
        this.TypeOfTween = type;
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

    public TweenData(Entity target, float4 to, float lifetime)
    {
        this.TypeOfTween = TypeOfTween.None;
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

    public override int GetHashCode()
    {
        return HashCode.Combine<int, Entity>((int)TypeOfTween, TweenEntity);
    }

    /// <summary>
    /// 动画类型标签
    /// </summary>
    public TypeOfTween TypeOfTween { get; }
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


    public TweenData SetDelay(float delay)
    {
        this.PassTime -= delay;
        return this;
    }

    public TweenData SetEase(Ease ease)
    {
        this.ease = ease;
        return this;
    }

    public TweenData SetIsRelative(bool isRelative)
    {
        this.isRelative = isRelative;
        return this;
    }

    public TweenData FromValue(float4 value)
    {
        this.From = value;
        return this;
    }
}
