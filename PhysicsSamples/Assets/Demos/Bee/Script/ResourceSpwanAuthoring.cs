using System.Collections.Generic;
using Unity.Collections;
using Unity.Collections.LowLevel.Unsafe;
using Unity.Transforms;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using UnityEngine;
using Collider = Unity.Physics.Collider;


struct SpawnResourceSettings : ISpawnSettings, IComponentData
{
    #region ISpawnSettings
    public Entity Prefab { get; set; }
    public float3 Position { get; set; }
    public quaternion Rotation { get; set; }
    public float3 Range { get; set; }
    public int Count { get; set; }
    public RandomType randomType { get; set; }
    #endregion

    public int Id;
    public int Countdown;
    public float Force;
    public Entity Source;
}

class ResourceSpwanAuthoring : SpawnRandomObjectsAuthoringBase<SpawnResourceSettings>
{
}
/// <summary>
/// 生成掉落资源
/// </summary>
class SpawnResourceSystem : SpawnRandomObjectsSystemBase<SpawnResourceSettings>
{
    protected override void ConfigureInstance(Entity instance, ref SpawnResourceSettings spawnSettings)
    {
        EntityManager.AddComponentData(instance, new DropResourceComponent
        {
        });
        //无法应用物理效果?
        //var pv = EntityManager.GetComponentData<PhysicsVelocity>(instance);
        //var pm = EntityManager.GetComponentData<PhysicsMass>(instance);
        //var t = EntityManager.GetComponentData<Translation>(instance);
        //var r = EntityManager.GetComponentData<Rotation>(instance);
        //var random = new Unity.Mathematics.Random((uint)UnityEngine.Time.time + 1);
        //var force = math.up() * 100 + random.NextFloat3();
        //pv.ApplyImpulse(pm, t, r, force, t.Value);
    }
}
