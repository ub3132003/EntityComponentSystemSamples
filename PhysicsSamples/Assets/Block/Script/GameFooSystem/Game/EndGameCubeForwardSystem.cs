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
        EntityCommandBufferSystem sys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        EntityCommandBuffer ecb = sys.CreateCommandBuffer();
        if (GameEventsContext.Instance.TriggleWaveStart)
        {
            Entities.ForEach((Entity entity, in BrickOutBoundsArea brickEndGame, in Translation translation) => {
                translation.DOMove(entity, ecb, math.forward(), 1f, isRelative: true);
            }).Schedule();
            GameEventsContext.Instance.TriggleWaveStart = false;
        }
    }
}
