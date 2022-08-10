using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
public enum RandomType
{
    RandomInRange,
    CellAtGrid,
}
class SpawnRandomObjectsAuthoring : SpawnRandomObjectsAuthoringBase<SpawnSettings>
{
}


abstract class SpawnRandomObjectsAuthoringBase<T> : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
    where T : struct, IComponentData, ISpawnSettings
{
    #pragma warning disable 649
    public GameObject prefab;
    public float3 range = new float3(10f);
    [Tooltip("Limited to 500 on some platforms!")]
    public int count;
    #pragma warning restore 649

    public RandomType randomType;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnSettings = new T
        {
            Prefab = conversionSystem.GetPrimaryEntity(prefab),
            Position = transform.position,
            Rotation = transform.rotation,
            Range = range,
            Count = count,
            randomType = randomType,
        };

        Configure(ref spawnSettings, entity, dstManager, conversionSystem);
        Configure(ref spawnSettings);
        dstManager.AddComponentData(entity, spawnSettings);
    }

    internal virtual void Configure(ref T spawnSettings) {}
    internal virtual void Configure(ref T spawnSettings, Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem) {}
    internal virtual void Configure(List<GameObject> referencedPrefabs) { referencedPrefabs.Add(prefab); }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => Configure(referencedPrefabs);
}

interface ISpawnSettings
{
    Entity Prefab { get; set; }
    float3 Position { get; set; }
    quaternion Rotation { get; set; }
    float3 Range { get; set; }
    int Count { get; set; }
    RandomType randomType { get; set; }
}

struct SpawnSettings : IComponentData, ISpawnSettings
{
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }
    public RandomType randomType { get; set; }
}

class SpawnRandomObjectsSystem : SpawnRandomObjectsSystemBase<SpawnSettings>
{
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
abstract partial class SpawnRandomObjectsSystemBase<T> : SystemBase where T : struct, IComponentData, ISpawnSettings
{
    internal virtual int GetRandomSeed(T spawnSettings)
    {
        var seed = 0;
        seed = (seed * 397) ^ spawnSettings.Count;
        seed = (seed * 397) ^ (int)math.csum(spawnSettings.Position);
        seed = (seed * 397) ^ (int)math.csum(spawnSettings.Range);
        return seed;
    }

    internal virtual void OnBeforeInstantiatePrefab(ref T spawnSettings) {}

    internal virtual void ConfigureInstance(Entity instance, ref T spawnSettings) {}

    protected override void OnUpdate()
    {
        // Entities.ForEach in generic system types are not supported
        using (var entities = GetEntityQuery(new ComponentType[] { typeof(T) }).ToEntityArray(Allocator.TempJob))
        {
            for (int j = 0; j < entities.Length; j++)
            {
                var entity = entities[j];
                var spawnSettings = EntityManager.GetComponentData<T>(entity);

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // Limit the number of bodies on platforms with potentially low-end devices
                var count = math.min(spawnSettings.Count, 500);
#else
                var count = spawnSettings.Count;
#endif

                OnBeforeInstantiatePrefab(ref spawnSettings);

                var instances = new NativeArray<Entity>(count, Allocator.Temp);
                EntityManager.Instantiate(spawnSettings.Prefab, instances);

                var positions = new NativeArray<float3>(count, Allocator.Temp);
                var rotations = new NativeArray<quaternion>(count, Allocator.Temp);
                switch (spawnSettings.randomType)
                {
                    case RandomType.RandomInRange:
                        RandomPointsInRange(spawnSettings.Position, spawnSettings.Rotation, spawnSettings.Range, ref positions, ref rotations, GetRandomSeed(spawnSettings));
                        break;
                    case RandomType.CellAtGrid:
                        RandomGrid(spawnSettings.Range, ref positions, GetRandomSeed(spawnSettings));
                        break;
                    default:
                        break;
                }

                for (int i = 0; i < count; i++)
                {
                    var instance = instances[i];
                    EntityManager.SetComponentData(instance, new Translation { Value = positions[i] });
                    if (spawnSettings.randomType == RandomType.RandomInRange)
                        EntityManager.SetComponentData(instance, new Rotation { Value = rotations[i] });

                    ConfigureInstance(instance, ref spawnSettings);
                }

                EntityManager.RemoveComponent<T>(entity);
            }
        }
    }

    protected static void RandomPointsInRange(
        float3 center, quaternion orientation, float3 range,
        ref NativeArray<float3> positions, ref NativeArray<quaternion> rotations, int seed = 0)
    {
        var count = positions.Length;
        // initialize the seed of the random number generator
        var random = new Unity.Mathematics.Random((uint)seed);
        for (int i = 0; i < count; i++)
        {
            positions[i] = center + math.mul(orientation, random.NextFloat3(-range, range));
            rotations[i] = math.mul(random.NextQuaternionRotation(), orientation);
        }
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="range"> y 高度 向下去整 例 2 ，0；1 </param>
    /// <param name="numTilesPerLine"></param>
    /// <param name="positions"></param>
    /// <param name="rotations"></param>
    /// <param name="seed"></param>
    protected static void RandomGrid(float3 range,
        ref NativeArray<float3> positions,  int seed = 0)
    {
        TerrainGeneration.NoiseSettings terrainNoise = new TerrainGeneration.NoiseSettings();
        terrainNoise.seed = seed;
        terrainNoise.numLayers = (int)range.y;

        var numTilesPerLine = (int)math.max(range.x, range.z);
        float[,] map = TerrainGeneration.HeightmapGenerator.GenerateHeightmap(terrainNoise, numTilesPerLine);

        var count = positions.Length;
        var center = float3.zero;
        // 可能数量不足 count个
        int i = 0;

        for (int z = 0; z < range.z; z++)
        {
            for (int x = 0; x < range.x; x++)
            {
                if (i >= count) return;
                int y = 0;
                y = (int)(map[x, z] * range.y);
                if (y == 0)
                {
                    continue;
                }
                for (int j = 1; j <= y; j++)
                {
                    positions[i++] = center + new float3(x, j, z);
                }
            }
        }
    }
}
