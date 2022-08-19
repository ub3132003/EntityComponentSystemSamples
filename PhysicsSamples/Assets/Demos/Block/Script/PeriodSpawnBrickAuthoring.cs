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
    protected override void InitTransform(float3 center, quaternion orientation, float3 range, ref NativeArray<float3> positions, ref NativeArray<quaternion> rotations, int seed = 1)
    {
        var random = new Unity.Mathematics.Random((uint)seed + 1);
        //var x0 = -range.x / 2;
        //var z0 = -range.z / 2;
        //var MaxVerticalOffset = 3;
        //var bounds = new BoundsInt(Vector3Int.zero, Vector3Int.one);
        //var p = new NativeArray<float3>((int)range.x * (int)range.z, Allocator.TempJob);
        //for (var x = 0; x < range.x; x++)
        //{
        //    for (var z = 0; z < range.z; z++)
        //    {
        //        int index = z + x * (int)range.z;
        //        var vertNoise = noise.cnoise(new float2(x + x0, z + z0) * 0.36f);
        //        p[index] =
        //            new float3((x + x0) * bounds.size.x * 1.1f, vertNoise * MaxVerticalOffset, (z + z0) * bounds.size.z * 1.1f);
        //    }
        //}
        //p.Dispose();
        for (int i = 0; i < positions.Length; i++)
        {
            positions[i] = center + random.NextInt3(-(int3)range, (int3)range);
        }
    }
}
