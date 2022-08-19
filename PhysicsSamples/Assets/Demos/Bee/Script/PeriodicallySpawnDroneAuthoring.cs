using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
public struct PeriodicallySpawnDroneComponent : IComponentData, ISpawnSettings, IPeriodicSpawnSettings
{
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }

    public int SpawnRate { get; set; }
    public int DeathRate { get; set; }
    public RandomType randomType { get; set; }

    public int Id;
}
/// <summary>
/// 配置生成属性
/// </summary>
public class PeriodicallySpawnDroneAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public float3 Range;
    public int SpawnRate;
    public GameObject Prefab;
    public int Count = 1;
    public int Lifetime = 500;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new PeriodicallySpawnDroneComponent
        {
            Count = Count,
            DeathRate = Lifetime,
            Position = transform.position,
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
            Range = Range,
            Rotation = quaternion.identity,
            SpawnRate = SpawnRate,
            Id = 0,
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(Prefab);
}

class PeriodicallySpawnDroneSystem : PeriodicalySpawnRandomObjectsSystem<PeriodicallySpawnDroneComponent>
{
    protected override void ConfigureInstance(Entity instance, ref PeriodicallySpawnDroneComponent spawnSettings)
    {
        var pos = EntityManager.GetComponentData<Translation>(instance);
        EntityManager.AddComponentData<DroneComponent>(instance, new DroneComponent
        {
            Magnitude = 10.0f,
            Direction = -Vector3.forward,
            Offset = Vector3.zero,
            index = instance.Index
        });
    }
}
