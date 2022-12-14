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

    /// 已经过去的时间
    /// </summary>
    public float PassTime { get; set; }
    public Entity TweenEntity { get; set; }

    /// <summary>
    /// 开始时的值
    /// </summary>
    public float4 Start { get; set; }
    public float4 End { get; set; }
    public float Lifetime { get; set; }

    public Ease ease { get; set; }
    public bool isReset { get; set; }//完成时重置到form
    /// <summary>
    ///     增量
    /// </summary>
    public bool isRelative { get; set; }

    public bool isLoop { get; set; }
    public LoopMode loopMode { get; set; }
    /// <summary>
    /// 完成后移除动画
    /// </summary>
    public bool AutoKill { get; set; }

    public bool IsComplete { get; }

    public void SetToValue(float3 to);
    public void SetToValue(float4 to);

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
    public static void CreateHDRColorTween(Entity tweenTarget, float4 to, float lifetime, Ease ease, bool isReset = false, float4 start = default, bool isRelative = false, bool autoKill = false)
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
            AutoKill = autoKill,

            To = to,
        });
    }

    public static void CreateTween(Entity tweenTarget, float4 from, float4 to, float lifetime, Ease ease, bool isReset = false , float4 start = default, bool isRelative = false, bool autoKill = false)
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
                AutoKill = autoKill,

                From = from,
                To = to,
            });
        }
        else
        {
            var tweener = new TweenHDRColorComponent
            {
                Lifetime = lifetime,
                PassTime = 0,
                ease = ease,
                isReset = isReset,
                Start = start,
                isRelative = isRelative,

                From = from,
                To = to,
            };
            if ((start == default).IsTure())
            {
                var color = entityManager.GetComponentData<URPMaterialPropertyEmissionColor>(tweenTarget);
                tweener.Start = color.Value;
            }
            entityManager.AddComponentData(tweenTarget, tweener);
        }
    }

    public static ITweenComponent CreateMoveTween(Entity tweenTarget, float3 to, float lifetime, Ease ease, bool isReset = default, float3 start = default , bool isRelative = false , bool autoKill = false)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;

        var tweener = new TweenPositionComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset,
            Start = new float4(start, 0),
            isRelative = isRelative,
            AutoKill = autoKill,

            To = to,
        };
        if ((start == default).IsTure())
        {
            var translation = entityManager.GetComponentData<Translation>(tweenTarget);
            tweener.Start = new float4(translation.Value, 0);
        }
        entityManager.AddComponentData(tweenTarget, tweener);
        return tweener;
    }

    public static TweenPositionComponent CreateMoveTween(Entity tweenTarget, float3 to, float lifetime)
    {
        var tweener = new TweenPositionComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = DOTween.defaultEaseType,
            isReset = false,
            Start = new float4(),
            isRelative = false,
            AutoKill = DOTween.defaultAutoKill,

            To = to,
        };
        return tweener;
    }

    public static T CreateTween<T>(Entity tweenTarget, float3 to, float lifetime) where T :  struct , ITweenComponent
    {
        var tweener = new T
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = DOTween.defaultEaseType,
            isReset = false,
            Start = new float4(),
            isRelative = false,
            AutoKill = DOTween.defaultAutoKill,
        };
        tweener.SetToValue(to);
        return tweener;
    }

    public void SetDelay(Entity tweenTarget, float delay);
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
public partial class TweenSystem : SystemBase
{
    static float4 TweenHdrColor(float4 hdrColor , float datleTime, ref TweenHDRColorComponent tweenHdr)
    {
        if (tweenHdr.PassTime == 0)
        {
            tweenHdr.Start = hdrColor;
        }
        if (tweenHdr.Lifetime > 0)
        {
            tweenHdr.PassTime += datleTime;

            var v = EaseManager.Evaluate(tweenHdr.ease, null, tweenHdr.PassTime, tweenHdr.Lifetime, 0, 0); //类变量导致无法bust编译
                                                                                                           //默认从当前值开始
            var from = tweenHdr.From.IsZero() ?
                tweenHdr.Start : tweenHdr.From;

            hdrColor = math.lerp(from, tweenHdr.To, v);

            if (tweenHdr.PassTime >= tweenHdr.Lifetime)
            {
                if (tweenHdr.isLoop)
                {
                    tweenHdr.PassTime = 0;
                }
                else
                {
                    tweenHdr.Lifetime = 0;
                    if (tweenHdr.isReset)
                    {
                        hdrColor = tweenHdr.Start;
                    }
                }
            }
        }
        return hdrColor;
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
            .WithoutBurst()
            .ForEach((Entity e , ref TweenHDRColorComponent tweenHdr) =>
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

                hdrColor = TweenHdrColor(hdrColor , datleTime, ref tweenHdr);

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
            .WithoutBurst()
            .ForEach((ref Translation translation, ref TweenPositionComponent tweenPosition) =>
            {
                tweenPosition.PassTime += datleTime;
                if (tweenPosition.Lifetime > 0 && tweenPosition.PassTime >= 0)
                {
                    var v = EaseManager.Evaluate(tweenPosition.ease, null, tweenPosition.PassTime, tweenPosition.Lifetime, 0, 0);
                    var from = tweenPosition.From.IsZero() ? tweenPosition.Start.xyz : tweenPosition.From;
                    var to = tweenPosition.isRelative ? tweenPosition.Start.xyz + tweenPosition.To : tweenPosition.To;
                    translation.Value = math.lerp(from, to, v);
                    if (tweenPosition.PassTime >= tweenPosition.Lifetime)
                    {
                        if (tweenPosition.isLoop)
                        {
                            tweenPosition.PassTime = 0;
                            if (tweenPosition.loopMode == LoopMode.Yoyo)
                            {
                                tweenPosition.To = -tweenPosition.To;
                            }
                        }
                        else
                        {
                            tweenPosition.Lifetime = 0;
                        }
                    }
                }
            }).Schedule();
        using (var commandBuffer = new EntityCommandBuffer(Allocator.TempJob))
        {
            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in TweenHDRColorComponent tween) =>
                {
                    RemoveTwenn(commandBuffer, entity, tween);
                }).Run();

            Entities
                .WithoutBurst()
                .ForEach((Entity entity, in TweenPositionComponent tween) =>
                {
                    RemoveTwenn(commandBuffer, entity, tween);
                }).Run();
            commandBuffer.Playback(EntityManager);
        }


        //auto kill

        //new TweenJob()
        //{
        //    hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
        //};
    }

    static void RemoveTwenn<T>(EntityCommandBuffer commandBuffer, Entity entity, T tween) where T : ITweenComponent
    {
        if (tween.AutoKill && tween.IsComplete)
        {
            commandBuffer.RemoveComponent<T>(entity);
        }
    }

    private struct TweenJob<T> : IJobEntity where T : struct
    {
        public float detleTime;

        public void Execute(Entity entity, ref ITweenComponent tween, T tweenComponent)
        {
            //tween.PassTime += detleTime;

            //var v = EaseManager.Evaluate(tween.ease, null, tween.PassTime, tween.Lifetime, 0, 0);
            //var from = tweenComponent.From.IsZero() ? tweenComponent.Value : tweenComponent.From;
            //var to = tween.isRelative ? from + tweenComponent.To : tweenComponent.To;
            //tweenComponent.Value = math.lerp(from, to, v);
        }
    }
}
