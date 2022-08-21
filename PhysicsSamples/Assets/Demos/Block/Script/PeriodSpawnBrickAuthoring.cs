using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;

using UnityEngine;

struct PeriodicSpawnBrickSettings : IComponentData, ISpawnSettings, IPeriodicSpawnSettings
{
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }
    public int SpawnRate { get; set; }
    public int DeathRate { get; set; }
    public RandomType randomType { get; set; }
}

class PeriodSpawnBrickAuthoring : SpawnRandomObjectsAuthoringBase<PeriodicSpawnBrickSettings>
{
    public int SpawnRate = 50;
    /// <summary>
    /// 小于等于0时不会销毁
    /// </summary>
    public int DeathRate = 50;

    protected override void Configure(ref PeriodicSpawnBrickSettings spawnSettings)
    {
        spawnSettings.SpawnRate = SpawnRate;
        spawnSettings.DeathRate = DeathRate;
    }
}


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
class PeriodBrickSpawnSystem : PeriodicalySpawnRandomObjectsSystem<PeriodicSpawnBrickSettings>
{
    EntityQuery queryOldBrickGroup;

    protected override void OnCreate()
    {
        var queryDescription = new EntityQueryDesc
        {
            All = new ComponentType[] { typeof(BrickComponent),
                                        ComponentType.ReadOnly<Translation>() }
        };
        queryOldBrickGroup = GetEntityQuery(queryDescription);
    }

    protected override void OnBeforeInstantiatePrefab(ref PeriodicSpawnBrickSettings spawnSettings)
    {
    }

    protected override void InitTransform(float3 center, quaternion orientation, float3 range, ref NativeArray<float3> positions, ref NativeArray<quaternion> rotations, int seed = 1)
    {
        var random = new Unity.Mathematics.Random((uint)seed + 1);
        var oldBriks = queryOldBrickGroup.ToComponentDataArray<Translation>(Allocator.Temp);

        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = center + random.NextInt3(-(int3)range, (int3)range);

            for (int j = 0; j < 3; j++)
            {
                for (int k = 0; k < oldBriks.Length; k++)
                {
                    if ((oldBriks[k].Value == positions[i]).IsTure())
                    {
                        positions[i] = center + random.NextInt3(-(int3)range, (int3)range);
                    }
                }
            }
        }
    }
}
