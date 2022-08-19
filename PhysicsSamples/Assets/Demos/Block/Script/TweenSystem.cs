using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using DG.Tweening.Core.Easing;
using DG.Tweening;
public interface ITweenComponent
{
    //public enum Type
    //{
    //    Position,
    //    Rotation,
    //    Scale,
    //    Color,
    //    HdrColor,
    //}
    /// <summary>
    /// 已经过去的时间
    /// </summary>
    public float PassTime { get; set; }
    public Entity TweenEntity { get; set; }


    public float4 Start { get; set; }
    public float4 End { get; set; }
    public float Lifetime { get; set; }

    public Ease ease { get; set; }
    public bool isReset { get; set; }//完成时重置到form
    public bool isRelative { get; set; }
    //TODO 重复触发问题, 在动画进行时再次触发了该动作该如何处理
    /// <summary>
    ///
    /// </summary>
    /// <param name="tweenTarget"></param>
    /// <param name="to"></param>
    /// <param name="lifetime"></param>
    /// <param name="ease"></param>
    /// <param name="isReset"></param>
    /// <param name="start"></param>
    /// <param name="isRelative"> 是否增量</param>
    public static void CreateTween(Entity tweenTarget, float4 to, float lifetime, Ease ease, bool isReset = false, float4 start = default, bool isRelative = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.AddComponentData(tweenTarget, new TweenHDRColorComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset,
            Start = start,
            isRelative = isRelative,

            To = to,
        });
    }

    public static void CreateTween(Entity tweenTarget, float4 from, float4 to, float lifetime, Ease ease, bool isReset = false , float4 start = default, bool isRelative = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //var entity = entityManager.CreateEntity();
        //entityManager.AddComponentData<TweenComponent>(entity, new TweenComponent
        //{
        //    TweenEntity = tweenTarget,
        //    Lifetime = lifetime,
        //    From = from,
        //    To = to,
        //}) ;
        if (entityManager.HasComponent<TweenHDRColorComponent>(tweenTarget))
        {
            entityManager.SetComponentData(tweenTarget, new TweenHDRColorComponent
            {
                Lifetime = lifetime,
                PassTime = 0,
                ease = ease,
                isReset = isReset,
                Start = start,
                isRelative = isRelative,

                From = from,
                To = to,
            });
        }
        else
        {
            entityManager.AddComponentData(tweenTarget, new TweenHDRColorComponent
            {
                Lifetime = lifetime,
                PassTime = 0,
                ease = ease,
                isReset = isReset,
                Start = start,
                isRelative = isRelative,

                From = from,
                To = to,
            });
        }
    }

    public static void CreateMoveTween(Entity tweenTarget, float3 to, float lifetime, Ease ease, bool isReset = default, float3 start = default , bool isRelative = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;


        entityManager.AddComponentData(tweenTarget, new TweenPositionComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset,
            Start = new float4(start, 0),
            isRelative = isRelative,

            To = to,
        });
    }
}
#region 动画组件对象
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


    public float3 From { get; set; }
    public float3 To { get; set; }
}
public struct TweenHDRColorComponent : IComponentData, ITweenComponent
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


    public float4 From { get; set; }
    public float4 To { get; set; }
}
#endregion


public partial class TweenSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var datleTime = Time.DeltaTime;

        //hdr 颜色
        Entities
            .WithoutBurst()
            .ForEach((ref URPMaterialPropertyEmissionColor HDRColor, ref TweenHDRColorComponent tweenHdr) =>
            {
                if (tweenHdr.PassTime == 0)
                {
                    tweenHdr.Start = HDRColor.Value;
                }
                if (tweenHdr.Lifetime > 0)
                {
                    tweenHdr.PassTime += datleTime;

                    var v = EaseManager.Evaluate(tweenHdr.ease, null, tweenHdr.PassTime, tweenHdr.Lifetime, 0, 0); //类变量导致无法bust编译
                    //默认从当前值开始
                    var from = tweenHdr.From.IsZero() ?
                        tweenHdr.Start : tweenHdr.From;

                    HDRColor.Value = math.lerp(from, tweenHdr.To, v);

                    if (tweenHdr.PassTime >= tweenHdr.Lifetime)
                    {
                        tweenHdr.Lifetime = 0;
                        if (tweenHdr.isReset)
                        {
                            HDRColor.Value = tweenHdr.Start; //TOdo 短时连续触发会保持值,在闪烁时无法达到效果
                        }
                    }
                }
            }).Schedule();

        //位置
        Entities
            .WithoutBurst()
            .ForEach((ref Translation translation, ref TweenPositionComponent tweenPosition) =>
            {
                if (tweenPosition.PassTime == 0)
                {
                    tweenPosition.Start = new float4(translation.Value, 0);
                }
                if (tweenPosition.Lifetime > 0)
                {
                    tweenPosition.PassTime += datleTime;

                    var v = EaseManager.Evaluate(tweenPosition.ease, null, tweenPosition.PassTime, tweenPosition.Lifetime, 0, 0);
                    var from = tweenPosition.From.IsZero() ? tweenPosition.Start.xyz : tweenPosition.From;
                    var to = tweenPosition.isRelative ? tweenPosition.Start.xyz + tweenPosition.To : tweenPosition.To;
                    translation.Value = math.lerp(from, to, v);
                    if (tweenPosition.PassTime >= tweenPosition.Lifetime)
                    {
                        tweenPosition.Lifetime = 0;
                    }
                }
            }).Schedule();
        //new TweenJob()
        //{
        //    hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
        //};
    }

    private struct TweenJob<T> : IJobEntity where T : struct
    {
        public float detleTime;

        public void Execute(Entity entity , ref ITweenComponent tween , T tweenComponent)
        {
            //tween.PassTime += detleTime;

            //var v = EaseManager.Evaluate(tween.ease, null, tween.PassTime, tween.Lifetime, 0, 0);
            //var from = tweenComponent.From.IsZero() ? tweenComponent.Value : tweenComponent.From;
            //var to = tween.isRelative ? from + tweenComponent.To : tweenComponent.To;
            //tweenComponent.Value = math.lerp(from, to, v);
        }
    }
}
