using DG.Tweening;
using Unity.Entities;
using Unity.Mathematics;
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

    public bool AutoKill { get; set; }
    public bool isLoop { get; set; }
    public LoopMode loopMode { get; set; }

    public float4 From { get; set; }
    public float4 To { get; set; }

    public bool IsComplete
    {
        get { return PassTime > Lifetime; }
    }


    public void SetDelay(Entity tweenTarget, float delay)
    {
        var entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        var tweener = entityManager.GetComponentData<TweenHDRColorComponent>(tweenTarget);
        tweener.PassTime -= delay;
        entityManager.SetComponentData(tweenTarget, tweener);
    }
}
