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

    public float4 From;
    public float4 To;
    public float Lifetime;

    public Ease ease;
    public bool isReset;//完成时重置到form
}
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
public partial class TweenSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var datleTime = Time.DeltaTime;
        Entities
            .WithoutBurst()
            .ForEach((ref TweenComponent tween, ref URPMaterialPropertyEmissionColor HDRColor, in TweenHDRColorComponent tweenHdr) =>
            {
                if (tween.Lifetime > 0)
                {
                    tween.PassTime += datleTime;
                    var ease = tween.ease;
                    var v = EaseManager.Evaluate(ease, null, tween.PassTime, tween.Lifetime, 0, 0); //类变量导致无法bust编译
                    float4 value = math.lerp(tweenHdr.From, tweenHdr.To, v);
                    HDRColor.Value = value;
                    if (tween.PassTime >= tween.Lifetime)
                    {
                        tween.Lifetime = 0;
                        if (tween.isReset)
                        {
                            HDRColor.Value = tweenHdr.From;
                        }
                    }
                }
            }).Schedule();

        Entities
            .WithoutBurst()
            .ForEach((ref TweenComponent tween, ref Translation position, in TweenPositionComponent tweenPosition) =>
            {
                tween.PassTime += datleTime;
                var ease = tween.ease;
                var v = EaseManager.Evaluate(ease, null, tween.PassTime, tween.Lifetime, 0, 0);
                float3 value = math.lerp(tweenPosition.From, tweenPosition.To, v);
                position.Value = value;
            }).Schedule();
        //new TweenJob()
        //{
        //    hdrColorGroup = GetComponentDataFromEntity<URPMaterialPropertyEmissionColor>(),
        //};
    }

    private struct TweenJob : IJobEntity
    {
        float detleTime;
        public ComponentDataFromEntity<URPMaterialPropertyEmissionColor> hdrColorGroup;
        public void Execute(Entity entity , ref TweenComponent tween)
        {
        }
    }

    public static void CreateTween(Entity tweenTarget, float4 from, float4 to, float lifetime, Ease ease, bool isReset)
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
        entityManager.AddComponentData(tweenTarget, new TweenComponent
        {
            Lifetime = lifetime,
            PassTime = 0,
            ease = ease,
            isReset = isReset
        });
        entityManager.AddComponentData(tweenTarget, new TweenHDRColorComponent
        {
            From = from,
            To = to,
        });
    }
}
