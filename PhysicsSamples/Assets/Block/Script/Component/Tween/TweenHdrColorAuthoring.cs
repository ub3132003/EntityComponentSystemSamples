using DG.Tweening;
using Unity.Entities;
using Unity.Mathematics;

public struct TweenHDRColorComponent : IComponentData, ITweenComponent
{
    public TweenData Value { get; set; }
}

class TweenHdrColorAuthoring : TweenAuthoring<TweenHDRColorComponent>
{
    protected override TweenHDRColorComponent CreateComponent(TweenData tweenData)
    {
        return
            new TweenHDRColorComponent
            {
                Value = tweenData
            };
    }
}
