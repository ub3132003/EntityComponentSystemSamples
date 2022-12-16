using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using DG.Tweening.Core.Easing;
using DG.Tweening;
using System;

public interface ITweenComponent
{
    public TweenData Value { get; set; }

    ////public enum Type
    ////{
    ////    Position,
    ////    Rotation,
    ////    Scale,
    ////    Color,
    ////    HdrColor,
    ////}

    ///// 已经过去的时间
    ///// </summary>
    //public float PassTime { get; set; }
    //public Entity TweenEntity { get; set; }

    ///// <summary>
    ///// 开始时的值
    ///// </summary>
    //public float4 Start { get; set; }
    //public float4 End { get; set; }
    //public float Lifetime { get; set; }

    //public Ease ease { get; set; }
    //public bool isReset { get; set; }//完成时重置到form
    ///// <summary>
    /////     增量
    ///// </summary>
    //public bool isRelative { get; set; }

    //public bool isLoop { get; set; }
    //public LoopMode loopMode { get; set; }
    ///// <summary>
    ///// 完成后移除动画
    ///// </summary>
    //public bool AutoKill { get; set; }

    //public bool IsComplete { get; }

    //public void SetToValue(float3 to);
    //public void SetToValue(float4 to);


    /// <summary>
    /// URPMaterialPropertyEmissionColor 修改
    /// </summary>
    /// <param name="tweenTarget"></param>
    /// <param name="from"></param>
    /// <param name="to"></param>
    /// <param name="lifetime"></param>
    /// <param name="ease"></param>
    /// <param name="isReset"></param>
    /// <param name="start"></param>
    /// <param name="isRelative"></param>
    /// <param name="autoKill"></param>


    //public void SetDelay(Entity tweenTarget, float delay);
}
class TweenerFactorty<T> where T : struct, IComponentData
{
    protected T tween;
    protected Entity tweenTarget;

    public TweenerFactorty()
    {
    }

    public TweenerFactorty(Entity tweenTarget, EntityCommandBuffer tweenEcb)
    {
        this.tweenTarget = tweenTarget;
        TweenEcb = tweenEcb;
    }

    public EntityCommandBuffer TweenEcb { get; set; }

    public void To(TweenPositionComponent tweenPosition)
    {
        TweenEcb.AddComponent(tweenTarget, tweenPosition);
    }

    public void CreateTween<T1, T2>()
    {
        //DOTween.Do

        TweenEcb.AddComponent(tweenTarget , tween);
    }
}
class MoveTweenerFactorty : TweenerFactorty<TweenPositionComponent>
{
    public MoveTweenerFactorty(Entity tweenTarget, EntityCommandBuffer tweenEcb) : base(tweenTarget, tweenEcb)
    {
    }
}
class HdrColorTweenerFactorty : TweenerFactorty<TweenHDRColorComponent>
{
}

#region 动画组件对象
public enum LoopMode
{
    Restart,
    Yoyo,
    Incremental,//叠加模式
}


#endregion
[UpdateInGroup(typeof(TweenSystemGroup))]
public partial class TweenCreateSystem : SystemBase
{
    NativeParallelHashMap<int, TweenData> tweenWaitCreateMap;

    public NativeParallelHashMap<int, TweenData> TweenWaitCreateMap { get => tweenWaitCreateMap; }

    protected override void OnCreate()
    {
        base.OnCreate();
        tweenWaitCreateMap = new NativeParallelHashMap<int, TweenData>(16, Allocator.Persistent);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        tweenWaitCreateMap.Dispose();
    }

    protected override void OnUpdate()
    {
        var waitCreateCount = tweenWaitCreateMap.Count();
        if (waitCreateCount <= 0)
        {
            return;
        }
        var keys = tweenWaitCreateMap.GetKeyArray(Allocator.Temp);
        using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
        {
            for (int i = 0; i < waitCreateCount; i++)
            {
                var key = keys[i];
                var value = tweenWaitCreateMap[key];
                switch (value.TypeOfTween)
                {
                    case TypeOfTween.None:
                        break;
                    case TypeOfTween.Position:
                        AddTweenComponent<TweenPositionComponent>(commandBuffer, value);
                        break;
                    case TypeOfTween.Color:
                        break;
                    case TypeOfTween.HdrColor:
                        break;
                    default:
                        break;
                }

                tweenWaitCreateMap.Remove(key);
            }

            commandBuffer.Playback(EntityManager);
        }
    }

    private TweenData AddTween<T>(T tween) where T : struct, IComponentData, ITweenComponent
    {
        tweenWaitCreateMap.Add(tween.Value.GetHashCode(), tween.Value);
        return tween.Value;
    }

    private TweenData AddTween(NativeParallelHashMap<int, TweenData> map, TweenData tweenData)
    {
        tweenWaitCreateMap.Add(tweenData.GetHashCode(), tweenData);
        return tweenData;
    }

    private TweenData AddTween<T>(TypeOfTween typeOfTween, Entity tweenTarget, float4 to, float lifetime) where T : struct, IComponentData, ITweenComponent
    {
        var tween = CreateTween<T>(typeOfTween, tweenTarget, to, lifetime);
        tweenWaitCreateMap.Add(tween.GetHashCode(), tween.Value);
        return tween.Value;
    }

    public static void AddTweenComponent<T>(EntityCommandBuffer commandBuffer, TweenData tween) where T : struct, IComponentData, ITweenComponent
    {
        commandBuffer.AddComponent<T>(tween.TweenEntity, new T { Value = tween});
    }

    public static T CreateTween<T>(TypeOfTween tweenType, Entity tweenTarget, float4 to, float lifetime) where T : struct, ITweenComponent
    {
        var tweener = new T
        {
            Value = new TweenData(tweenType, tweenTarget, to, lifetime),
        };

        return tweener;
    }

    public static void CreateHdrColorTween(Entity tweenTarget, float4 from, float4 to, float lifetime, DG.Tweening.Ease ease, bool isReset = false, float4 start = default, bool isRelative = false, bool autoKill = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var tweener = CreateTween<TweenHDRColorComponent>(TypeOfTween.HdrColor , tweenTarget, to, lifetime);
        var value = tweener.Value;
        value.ease = ease;
        value.isReset = isReset;

        value.isRelative = isRelative;
        value.AutoKill = autoKill;

        if ((start == default).IsTure())
        {
            var color = entityManager.GetComponentData<URPMaterialPropertyEmissionColor>(tweenTarget);
            value.Start = color.Value;
        }

        tweener.Value = value;
        entityManager.AddComponentData(tweenTarget, tweener);
    }

    public static void CreateMoveTween(Entity tweenTarget, float3 to, float lifetime, DG.Tweening.Ease ease, float3 from = default, bool isReset = false, float4 start = default, bool isRelative = false, bool autoKill = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var tweener = CreateTween<TweenPositionComponent>(TypeOfTween.Position, tweenTarget, new float4(to, 0), lifetime);
        var value = tweener.Value;
        value.ease = ease;
        value.isReset = isReset;

        value.isRelative = isRelative;
        value.AutoKill = autoKill;

        if ((start == default).IsTure())
        {
            var translation = entityManager.GetComponentData<Translation>(tweenTarget);
            value.Start = new float4(translation.Value, 0);
        }

        tweener.Value = value;
        entityManager.AddComponentData(tweenTarget, tweener);
    }
}

[UpdateInGroup(typeof(TweenSystemGroup))]
[UpdateBefore(typeof(TweenSystem))]
public partial class TweenRemoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //删除完成的动画
        using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
        {
            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in TweenHDRColorComponent tween) =>
                {
                    RemoveTwenn<TweenHDRColorComponent>(commandBuffer, entity, tween.Value);
                }).Run();

            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in TweenPositionComponent tween) =>
                {
                    RemoveTwenn<TweenPositionComponent>(commandBuffer, entity, tween.Value);
                }).Run();
            commandBuffer.Playback(EntityManager);
        }
    }

    static void RemoveTwenn<T>(EntityCommandBuffer commandBuffer, Entity entity, TweenData tween) where T : struct, IComponentData, ITweenComponent
    {
        if (tween.AutoKill && tween.IsComplete)
        {
            commandBuffer.RemoveComponent<T>(entity);
        }
    }
}

[UpdateInGroup(typeof(TweenSystemGroup))]
[UpdateBefore(typeof(TweenCreateSystem))]
public partial class TweenSystem : SystemBase
{
    static float4 TweenValueTo(ref TweenData value, float datleTime)
    {
        float4 output = float4.zero;
        value.PassTime += datleTime;
        if (value.Duration > 0 && value.PassTime >= 0)// 校验条件不应该放在这里 TOdo
        {
            var v = Evaluate(value.ease, value.PassTime, value.Duration, 0, 0);
            var from = value.From.Equals(float4.zero) ? value.Start : value.From;
            var to = value.isRelative ? value.Start + value.To : value.To;
            output = math.lerp(from, to, v);
            if (value.PassTime >= value.Duration)
            {
                if (value.isLoop)
                {
                    value.PassTime = 0;
                    if (value.loopMode == LoopMode.Yoyo)
                    {
                        value.To = value.Start;
                    }
                }
            }
        }

        return output;
    }

    protected override void OnUpdate()
    {
        var datleTime = Time.DeltaTime;
        var em = World.DefaultGameObjectInjectionWorld.EntityManager;


        //hdr 颜色
        Entities
            .WithName("TweenHdrColor")
            .WithAll<TweenHDRColorComponent>()
            .WithAny<URPMaterialPropertyEmissionColor, EmissionVector4Override>()
            .ForEach((Entity e, ref TweenHDRColorComponent tweenHdr) =>
            {
                float4 hdrColor = new float4();
                if (HasComponent<URPMaterialPropertyEmissionColor>(e))
                {
                    hdrColor = GetComponent<URPMaterialPropertyEmissionColor>(e).Value;
                }
                if (HasComponent<EmissionVector4Override>(e))
                {
                    hdrColor = GetComponent<EmissionVector4Override>(e).Value;
                }
                var value = tweenHdr.Value;
                hdrColor = TweenValueTo(ref value, datleTime);
                tweenHdr.Value = value;

                if (HasComponent<URPMaterialPropertyEmissionColor>(e))
                {
                    SetComponent(e, new URPMaterialPropertyEmissionColor { Value = hdrColor });
                }
                if (HasComponent<EmissionVector4Override>(e))
                {
                    SetComponent(e, new EmissionVector4Override { Value = hdrColor });
                }
            }).Schedule();

        //位置
        Entities
            .ForEach((ref Translation translation, ref TweenPositionComponent tweenPosition) =>
        {
            if (tweenPosition.Value.IsComplete)
            {
                return;
            }
            var tweenData = tweenPosition.Value;
            translation.Value = TweenValueTo(ref tweenData, datleTime).xyz;
            tweenPosition.Value = tweenData;
            //tweenPosition.PassTime += datleTime;
            //if (tweenPosition.Lifetime > 0 && tweenPosition.PassTime >= 0)
            //{
            //    var v = EaseManager.Evaluate(tweenPosition.ease, null, tweenPosition.PassTime, tweenPosition.Lifetime, 0, 0);
            //    var from = tweenPosition.From.IsZero() ? tweenPosition.Start.xyz : tweenPosition.From;
            //    var to = tweenPosition.isRelative ? tweenPosition.Start.xyz + tweenPosition.To : tweenPosition.To;
            //    translation.Value = math.lerp(from, to, v);
            //    if (tweenPosition.PassTime >= tweenPosition.Lifetime)
            //    {
            //        if (tweenPosition.isLoop)
            //        {
            //            tweenPosition.PassTime = 0;
            //            if (tweenPosition.loopMode == LoopMode.Yoyo)
            //            {
            //                tweenPosition.To = -tweenPosition.To;
            //            }
            //        }
            //        else
            //        {
            //            tweenPosition.Lifetime = 0;
            //        }
            //    }
            //}
        }).Schedule();
    }

    private const float _PiOver2 = (float)Math.PI / 2f;

    private const float _TwoPi = (float)Math.PI * 2f;


    //
    // 摘要:
    //     Returns a value between 0 and 1 (inclusive) based on the elapsed time and ease
    //     selected
    public static float Evaluate(Ease easeType, float time, float duration, float overshootOrAmplitude, float period)
    {
        switch (easeType)
        {
            case Ease.Linear:
                return time / duration;
            case Ease.InSine:
                return 0f - (float)Math.Cos(time / duration * ((float)Math.PI / 2f)) + 1f;
            case Ease.OutSine:
                return (float)Math.Sin(time / duration * ((float)Math.PI / 2f));
            case Ease.InOutSine:
                return -0.5f * ((float)Math.Cos((float)Math.PI * time / duration) - 1f);
            case Ease.InQuad:
                return (time /= duration) * time;
            case Ease.OutQuad:
                return (0f - (time /= duration)) * (time - 2f);
            case Ease.InOutQuad:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * time * time;
                }

                return -0.5f * ((time -= 1f) * (time - 2f) - 1f);
            case Ease.InCubic:
                return (time /= duration) * time * time;
            case Ease.OutCubic:
                return (time = time / duration - 1f) * time * time + 1f;
            case Ease.InOutCubic:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * time * time * time;
                }

                return 0.5f * ((time -= 2f) * time * time + 2f);
            case Ease.InQuart:
                return (time /= duration) * time * time * time;
            case Ease.OutQuart:
                return 0f - ((time = time / duration - 1f) * time * time * time - 1f);
            case Ease.InOutQuart:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * time * time * time * time;
                }

                return -0.5f * ((time -= 2f) * time * time * time - 2f);
            case Ease.InQuint:
                return (time /= duration) * time * time * time * time;
            case Ease.OutQuint:
                return (time = time / duration - 1f) * time * time * time * time + 1f;
            case Ease.InOutQuint:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * time * time * time * time * time;
                }

                return 0.5f * ((time -= 2f) * time * time * time * time + 2f);
            case Ease.InExpo:
                if (time != 0f)
                {
                    return (float)Math.Pow(2.0, 10f * (time / duration - 1f));
                }

                return 0f;
            case Ease.OutExpo:
                if (time == duration)
                {
                    return 1f;
                }

                return 0f - (float)Math.Pow(2.0, -10f * time / duration) + 1f;
            case Ease.InOutExpo:
                if (time == 0f)
                {
                    return 0f;
                }

                if (time == duration)
                {
                    return 1f;
                }

                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * (float)Math.Pow(2.0, 10f * (time - 1f));
                }

                return 0.5f * (0f - (float)Math.Pow(2.0, -10f * (time -= 1f)) + 2f);
            case Ease.InCirc:
                return 0f - ((float)Math.Sqrt(1f - (time /= duration) * time) - 1f);
            case Ease.OutCirc:
                return (float)Math.Sqrt(1f - (time = time / duration - 1f) * time);
            case Ease.InOutCirc:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return -0.5f * ((float)Math.Sqrt(1f - time * time) - 1f);
                }

                return 0.5f * ((float)Math.Sqrt(1f - (time -= 2f) * time) + 1f);
            case Ease.InElastic:
            {
                if (time == 0f)
                {
                    return 0f;
                }

                if ((time /= duration) == 1f)
                {
                    return 1f;
                }

                if (period == 0f)
                {
                    period = duration * 0.3f;
                }

                float num3;
                if (overshootOrAmplitude < 1f)
                {
                    overshootOrAmplitude = 1f;
                    num3 = period / 4f;
                }
                else
                {
                    num3 = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / overshootOrAmplitude);
                }

                return 0f - overshootOrAmplitude * (float)Math.Pow(2.0, 10f * (time -= 1f)) * (float)Math.Sin((time * duration - num3) * ((float)Math.PI * 2f) / period);
            }
            case Ease.OutElastic:
            {
                if (time == 0f)
                {
                    return 0f;
                }

                if ((time /= duration) == 1f)
                {
                    return 1f;
                }

                if (period == 0f)
                {
                    period = duration * 0.3f;
                }

                float num2;
                if (overshootOrAmplitude < 1f)
                {
                    overshootOrAmplitude = 1f;
                    num2 = period / 4f;
                }
                else
                {
                    num2 = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / overshootOrAmplitude);
                }

                return overshootOrAmplitude * (float)Math.Pow(2.0, -10f * time) * (float)Math.Sin((time * duration - num2) * ((float)Math.PI * 2f) / period) + 1f;
            }
            case Ease.InOutElastic:
            {
                if (time == 0f)
                {
                    return 0f;
                }

                if ((time /= duration * 0.5f) == 2f)
                {
                    return 1f;
                }

                if (period == 0f)
                {
                    period = duration * 0.450000018f;
                }

                float num;
                if (overshootOrAmplitude < 1f)
                {
                    overshootOrAmplitude = 1f;
                    num = period / 4f;
                }
                else
                {
                    num = period / ((float)Math.PI * 2f) * (float)Math.Asin(1f / overshootOrAmplitude);
                }

                if (time < 1f)
                {
                    return -0.5f * (overshootOrAmplitude * (float)Math.Pow(2.0, 10f * (time -= 1f)) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period));
                }

                return overshootOrAmplitude * (float)Math.Pow(2.0, -10f * (time -= 1f)) * (float)Math.Sin((time * duration - num) * ((float)Math.PI * 2f) / period) * 0.5f + 1f;
            }
            case Ease.InBack:
                return (time /= duration) * time * ((overshootOrAmplitude + 1f) * time - overshootOrAmplitude);
            case Ease.OutBack:
                return (time = time / duration - 1f) * time * ((overshootOrAmplitude + 1f) * time + overshootOrAmplitude) + 1f;
            case Ease.InOutBack:
                if ((time /= duration * 0.5f) < 1f)
                {
                    return 0.5f * (time * time * (((overshootOrAmplitude *= 1.525f) + 1f) * time - overshootOrAmplitude));
                }

                return 0.5f * ((time -= 2f) * time * (((overshootOrAmplitude *= 1.525f) + 1f) * time + overshootOrAmplitude) + 2f);
            case Ease.InBounce:
                return Bounce.EaseIn(time, duration, overshootOrAmplitude, period);
            case Ease.OutBounce:
                return Bounce.EaseOut(time, duration, overshootOrAmplitude, period);
            case Ease.InOutBounce:
                return Bounce.EaseInOut(time, duration, overshootOrAmplitude, period);
            case Ease.INTERNAL_Custom:
                return 0;
            case Ease.INTERNAL_Zero:
                return 1f;
            case Ease.Flash:
                return Flash.Ease(time, duration, overshootOrAmplitude, period);
            case Ease.InFlash:
                return Flash.EaseIn(time, duration, overshootOrAmplitude, period);
            case Ease.OutFlash:
                return Flash.EaseOut(time, duration, overshootOrAmplitude, period);
            case Ease.InOutFlash:
                return Flash.EaseInOut(time, duration, overshootOrAmplitude, period);
            default:
                return (0f - (time /= duration)) * (time - 2f);
        }
    }
}
