using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using UnityEngine;
using Unity.Transforms;
public struct SpawnDroneSettings : IComponentData, ISpawnSettings
{
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }
    public float Restitution;

    public RandomType randomType { get; set; }

    public float MinBeeSize;
    public float MaxBeeSize;
    public float speedStretch;
    public float rotationStiffness;
    public float4 Color;

    public float aggression;
    public float flightJitter;
    public float teamAttraction;
    public float teamRepulsion;
    [Range(0f, 1f)]
    public float damping;

    public float chaseForce;
    public float carryForce;
    public float grabDistance;
    public float attackDistance;
    public float attackForce;
    public float hitDistance;
    public float maxSpawnSpeed;

    public float3 resourceDestination;
}

[DisallowMultipleComponent]
public class DroneSpawnAuthoring : SpawnRandomObjectsAuthoringBase<SpawnDroneSettings>
{
    public Mesh beeMesh;
    public Material beeMaterial;
    public Color[] teamColors;
    public float minBeeSize = 0.5f;
    public float maxBeeSize = 1f;
    public float speedStretch = 0.2f;
    public float rotationStiffness = 5;
    [Space(10)]
    [Range(0f, 1f)]
    public float aggression;
    public float flightJitter;
    public float teamAttraction;
    public float teamRepulsion;
    [Range(0f, 1f)]
    public float damping = 0.1f;
    public float chaseForce = 50f;
    public float carryForce = 25f;
    public float grabDistance = 0.5f;
    //大本营默认位置
    public float3 resourceDestination;
    protected override void Configure(ref SpawnDroneSettings spawnSettings)
    {
        spawnSettings.MinBeeSize = minBeeSize;
        spawnSettings.MaxBeeSize = maxBeeSize;
        spawnSettings.Color = teamColors[0].ToFloat4();
        spawnSettings.speedStretch = speedStretch;
        spawnSettings.flightJitter = flightJitter;
        spawnSettings.damping = damping;
        spawnSettings.rotationStiffness = rotationStiffness;
        spawnSettings.chaseForce = chaseForce;
        spawnSettings.carryForce = carryForce;
        spawnSettings.grabDistance = grabDistance;
        spawnSettings.resourceDestination = resourceDestination;
    }
}

partial class DroneSpawnSystem : SpawnRandomObjectsSystemBase<SpawnDroneSettings>
{
    Unity.Mathematics.Random _random;
    protected override void OnBeforeInstantiatePrefab(ref SpawnDroneSettings spawnSettings)
    {
        _random = new Unity.Mathematics.Random((uint)GetRandomSeed(spawnSettings) + 1);
    }

    protected override void ConfigureInstance(Entity instance, ref SpawnDroneSettings spawnSettings, float3 position, quaternion rotation)
    {
        var drone = new Drone();

        drone.Init(position, 0, _random.NextFloat(spawnSettings.MinBeeSize, spawnSettings.MaxBeeSize));
        drone.index = instance.Index;
        drone.resourceDestination = spawnSettings.resourceDestination;
        EntityManager.AddComponentData(instance, drone);
        EntityManager.AddSharedComponentData(instance,
            new DroneSettings
            {
                speedStretch = spawnSettings.speedStretch,
                flightJitter = spawnSettings.flightJitter,
                damping = spawnSettings.damping,
                rotationStiffness = spawnSettings.rotationStiffness,
                chaseForce = spawnSettings.chaseForce,
                carryForce = spawnSettings.carryForce,
                grabDistance = spawnSettings.grabDistance,
            });;
        EntityManager.SetComponentData(instance, new URPMaterialPropertyBaseColor { Value = spawnSettings.Color });
        EntityManager.RemoveComponent<Rotation>(instance);
        EntityManager.RemoveComponent<Translation>(instance);
    }
}
