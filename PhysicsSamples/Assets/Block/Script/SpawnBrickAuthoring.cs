using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;

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
    private void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireCube(transform.position + (Vector3)range / 2, range);
    }
}

partial class BrickSpawnSystem : SpawnRandomObjectsSystemBase<SpawnBrickSettings>
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

    protected override void OnUpdate()
    {
        var oldBriks = queryOldBrickGroup.ToComponentDataArray<Translation>(Allocator.Temp);

        using (var entities = GetEntityQuery(new ComponentType[] { typeof(SpawnBrickSettings) }).ToEntityArray(Allocator.TempJob))
        {
            for (int j = 0; j < entities.Length; j++)
            {
                var entity = entities[j];
                var spawnSettings = EntityManager.GetComponentData<SpawnBrickSettings>(entity);

#if UNITY_ANDROID || UNITY_IOS || UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
                // Limit the number of bodies on platforms with potentially low-end devices
                var count = math.min(spawnSettings.Count, 500);
#else
                var count = spawnSettings.Count;
#endif

                OnBeforeInstantiatePrefab(ref spawnSettings);

                var positions = new NativeArray<float3>(count, Allocator.Temp);
                var rotations = new NativeArray<quaternion>(count, Allocator.Temp);
                int rowCount = (int)spawnSettings.Range.x;
                int colCount = (int)spawnSettings.Range.z;
                BrickSpawnSystem.TilePlane((int3)spawnSettings.Position, (int3)spawnSettings.Range, ref positions);

                //var instances = new NativeArray<Entity>(count, Allocator.Temp);


                for (int i = 0; i < count; i++)
                {
                    var find = false;
                    for (int k = 0; k < oldBriks.Length; k++)
                    {
                        if ((oldBriks[k].Value == positions[i]).IsTure())
                        {
                            find = true; break;
                        }
                    }
                    if (find) continue;

                    var instance = EntityManager.Instantiate(spawnSettings.Prefab);
                    EntityManager.SetComponentData(instance, new Translation { Value = positions[i] });
                    if (spawnSettings.randomType == RandomType.RandomInRange)
                        EntityManager.SetComponentData(instance, new Rotation { Value = rotations[i] });

                    ConfigureInstance(instance, ref spawnSettings);
                }


                EntityManager.RemoveComponent<SpawnBrickSettings>(entity);
            }
        }
    }
}
