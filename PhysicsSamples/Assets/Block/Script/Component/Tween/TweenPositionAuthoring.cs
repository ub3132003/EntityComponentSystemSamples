using DG.Tweening;
using System.Net.Sockets;
using Unity.Entities;
using Unity.Mathematics;


public struct TweenPositionComponent : IComponentData, ITweenComponent
{
    public TweenData Value { get; set; }
}

class TweenPositionAuthoring : TweenAuthoring<TweenPositionComponent>
{
    protected override TweenPositionComponent CreateComponent(TweenData tweenData)
    {
        return
            new TweenPositionComponent
            {
                Value = tweenData
            };
    }
}
