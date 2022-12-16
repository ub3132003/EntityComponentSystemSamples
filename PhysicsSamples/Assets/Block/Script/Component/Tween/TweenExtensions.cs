using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
namespace Tween
{
    public static class TweenExtensions
    {
        //无法burst

        public static TweenData DOMove(this Translation target, Entity entity, float3 endValue, float duration)
        {
            var tween = new TweenData(TypeOfTween.Position, entity, new float4(endValue, 0), duration);
            return tween;
        }
    }
}
