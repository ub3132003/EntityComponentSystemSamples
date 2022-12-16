using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Tween;
/// <summary>
/// 游戏末尾的一排方块。
/// </summary>
public partial class EndGameCubeForwardSystem : SystemBase
{
    protected override void OnUpdate()
    {
        EntityCommandBufferSystem sys = this.World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = sys.CreateCommandBuffer();

        if (GameEventsContext.Instance.TriggleWaveStart)
        {
            Entities.ForEach((Entity entity, in BrickOutBoundsArea brickEndGame, in Translation translation) => {
                var tween = new TweenData(TypeOfTween.Position, entity, new float4(math.forward(), 0), 1f)
                    .SetIsRelative(true);
                TweenCreateSystem.AddTweenComponent<TweenPositionComponent>(ecb, tween);
            }).Schedule();
            GameEventsContext.Instance.TriggleWaveStart = false;
        }
    }
}
