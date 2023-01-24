using Unity.Collections;
using Unity.Entities;
using Unity.Physics.Systems;
using UnityEngine;

public struct LifeTime : IComponentData
{
    public int Value;
}

public class LifeTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [Tooltip("The number of frames until the entity should be destroyed.")]
    public int Value;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) =>
        dstManager.AddComponentData(entity, new LifeTime { Value = Value });
}

public partial class LifeTimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var sys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
        var commandBuffer = sys.CreateCommandBuffer();

        Entities
            .WithName("DestroyExpiredLifeTime")
            .ForEach((Entity entity, ref LifeTime timer) =>
            {
                timer.Value -= 1;

                if (timer.Value < 0f)
                {
                    commandBuffer.DestroyEntity(entity);
                }
            }).Schedule();
        sys.AddJobHandleForProducer(Dependency);
    }
}
