using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
struct SpawnBrickSettings : IComponentData, ISpawnSettings
{
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }
    public float Restitution;

    public RandomType randomType { get; set; }
}
class SpawnBrickAuthoring : SpawnRandomObjectsAuthoringBase<SpawnBrickSettings>
{
}

partial class BrickSpawnSystem : SpawnRandomObjectsSystemBase<SpawnBrickSettings>
{
}
