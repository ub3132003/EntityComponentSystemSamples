using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
namespace Tween
{
    public static class TweenExtensions
    {
        public static void DOMove(this Translation target, Entity entity, EntityCommandBuffer ecb, float3 endValue, float duration, bool isRelative = false)
        {
            //var tween = ITweenComponent.CreateMoveTween(entity, endValue, duration);
            var tween = ITweenComponent.CreateTween<TweenPositionComponent>(entity, endValue, duration);

            tween.Start = new float4(target.Value, 0);

            tween.isRelative = isRelative;


            ecb.AddComponent(entity, tween);
        }
    }
}
