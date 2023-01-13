using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
public struct SpawnResourceSettings : IComponentData, ISpawnSettings
{
    public Entity       Prefab { get; set; }
    public float3       Position { get; set; }
    public quaternion   Rotation { get; set; }
    public float3       Range { get; set; }
    public int          Count { get; set; }
    public RandomType   randomType { get; set; }

    public float resourceSize;
    public float snapStiffness;
    public float carryStiffness;
    public float spawnRate;
    public int beesPerResource;
}

[DisallowMultipleComponent]
public class ResourceSpawnAuthoring : SpawnRandomObjectsAuthoringBase<SpawnResourceSettings>
{
    public float resourceSize = 1f;
    public float snapStiffness;
    public float carryStiffness;
    public float spawnRate;
    public int beesPerResource;
    protected override void Configure(ref SpawnResourceSettings spawnSettings)
    {
        spawnSettings.resourceSize = resourceSize;
        spawnSettings.snapStiffness = snapStiffness;
        spawnSettings.carryStiffness = carryStiffness;
    }
}

partial class ResourceSpawnSystem : SpawnRandomObjectsSystemBase<SpawnResourceSettings>
{
    protected override void ConfigureInstance(Entity instance, ref SpawnResourceSettings spawnSettings, float3 position, quaternion rotation)
    {
        var resource = new ResourceItem(position);
        EntityManager.AddComponentData(instance, resource);
        EntityManager.AddSharedComponentData(instance, new ResourceItemSetting
        {
            resourceSize = spawnSettings.resourceSize,
            snapStiffness = spawnSettings.snapStiffness,
            carryStiffness = spawnSettings.carryStiffness,
        });
    }
}
