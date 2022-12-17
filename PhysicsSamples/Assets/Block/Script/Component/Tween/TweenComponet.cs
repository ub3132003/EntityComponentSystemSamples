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
        PassTime = -1;
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
        PassTime = -1;
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
    /// 已经过去的时间 创建时为-1 以此设定start 的初值
    /// </summary>
    public float PassTime;
    public Entity TweenEntity;

    public float4 Start;
    public float4 End;
    public float Duration;
    public Ease ease;
    public bool isReset;//完成时重置到for
    public bool isRelative;
    public bool AutoKill;
    public bool isLoop;
    public LoopMode loopMode;
    public float4 From;
    public float4 To;

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
