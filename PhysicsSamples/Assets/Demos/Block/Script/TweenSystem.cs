using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using DG.Tweening.Core.Easing;
using DG.Tweening;
public struct TweenComponent : IComponentData
{
    public enum Type
    {
        Position,
        Rotation,
        Scale,
        Color,
        HdrColor,
    }
    /// <summary>
    /// 已经过去的时间
    /// </summary>
    public float PassTime;
    public Entity TweenEntity;
    public Type type;

    public float4 Start;
    public float4 End;
    public float Lifetime;

    public Ease ease;
    public bool isReset;//完成时重置到form
    public bool isRelative;
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

        entityManager.AddComponentData(tweenTarget, new TweenComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset,
            Start = start,
            isRelative = isRelative,
        });
        entityManager.AddComponentData(tweenTarget, new TweenHDRColorComponent
        {
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
            entityManager.SetComponentData(tweenTarget, new TweenComponent
            {
                Lifetime = lifetime,
                PassTime = 0,
                ease = ease,
                isReset = isReset,
                Start = start,
                isRelative = isRelative,
            });
            entityManager.SetComponentData(tweenTarget, new TweenHDRColorComponent
            {
                From = from,
                To = to,
            });
        }
        else
        {
            entityManager.AddComponentData(tweenTarget, new TweenComponent
            {
                Lifetime = lifetime,
                PassTime = 0,
                ease = ease,
                isReset = isReset,
                Start = start,
                isRelative = isRelative,
            });
            entityManager.AddComponentData(tweenTarget, new TweenHDRColorComponent
            {
                From = from,
                To = to,
            });
        }
    }

    public static void CreateTween(Entity tweenTarget, float3 to, float lifetime, Ease ease, bool isReset = default, float3 start = default , bool isRelative = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        entityManager.AddComponentData(tweenTarget, new TweenComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset,
            Start = new float4(start, 0) ,
            isRelative = isRelative,
        });;
        entityManager.AddComponentData(tweenTarget, new TweenPositionComponent
        {
            To = to,
        });
    }
}
#region 动画组件对象
public struct TweenPositionComponent : IComponentData
{
    public float3 From;
    public float3 To;
}
public struct TweenHDRColorComponent : IComponentData
{
    public float4 From;
    public float4 To;
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
            .ForEach((ref TweenComponent tween, ref URPMaterialPropertyEmissionColor HDRColor, in TweenHDRColorComponent tweenHdr) =>
            {
                if (tween.PassTime == 0)
                {
                    tween.Start = HDRColor.Value;
                }
                if (tween.Lifetime > 0)
                {
                    tween.PassTime += datleTime;

                    var v = EaseManager.Evaluate(tween.ease, null, tween.PassTime, tween.Lifetime, 0, 0); //类变量导致无法bust编译
                    //默认从当前值开始
                    var from = tweenHdr.From.IsZero() ?
                        tween.Start : tweenHdr.From;

                    HDRColor.Value = math.lerp(from, tweenHdr.To, v);

                    if (tween.PassTime >= tween.Lifetime)
                    {
                        tween.Lifetime = 0;
                        if (tween.isReset)
                        {
                            HDRColor.Value = tween.Start; //TOdo 短时连续触发会保持值,在闪烁时无法达到效果
                        }
                    }
                }
            }).Schedule();

        //位置
        Entities
            .WithoutBurst()
            .ForEach((ref TweenComponent tween, ref Translation translation, in TweenPositionComponent tweenPosition) =>
            {
                if (tween.PassTime == 0)
                {
                    tween.Start = new float4(translation.Value, 0);
                }
                if (tween.Lifetime > 0)
                {
                    tween.PassTime += datleTime;

                    var v = EaseManager.Evaluate(tween.ease, null, tween.PassTime, tween.Lifetime, 0, 0);
                    var from = tweenPosition.From.IsZero() ? tween.Start.xyz : tweenPosition.From;
                    var to = tween.isRelative ? tween.Start.xyz + tweenPosition.To : tweenPosition.To;
                    translation.Value = math.lerp(from, to, v);
                    if (tween.PassTime >= tween.Lifetime)
                    {
                        tween.Lifetime = 0;
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

        public void Execute(Entity entity , ref TweenComponent tween , T tweenComponent)
        {
            //tween.PassTime += detleTime;

            //var v = EaseManager.Evaluate(tween.ease, null, tween.PassTime, tween.Lifetime, 0, 0);
            //var from = tweenComponent.From.IsZero() ? tweenComponent.Value : tweenComponent.From;
            //var to = tween.isRelative ? from + tweenComponent.To : tweenComponent.To;
            //tweenComponent.Value = math.lerp(from, to, v);
        }
    }
}
