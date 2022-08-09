using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;

public struct BulletComponent : IComponentData
{
    public int Damage;
}

[DisallowMultipleComponent]
public class BulletAuthor : MonoBehaviour, IConvertGameObjectToEntity
{
    [Min(0)]
    public int Damage;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BulletComponent
        {
            Damage = Damage
        });
    }
}

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class TriggerBulletSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities
            .WithName("ApplyRocketThrust")
            .WithBurst()
            .ForEach((ref BulletComponent bullet, ref Translation t, ref Rotation r, ref PhysicsVelocity pv, ref PhysicsMass pm) =>
            {
                pv.Linear = math.normalize(pv.Linear) * 35;
            }).Schedule();
    }
}
