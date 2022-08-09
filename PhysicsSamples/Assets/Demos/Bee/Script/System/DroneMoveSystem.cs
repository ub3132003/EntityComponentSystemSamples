using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Authoring;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;


[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(BuildPhysicsWorld))]
public partial class DroneMoveSystem : SystemBase
{
    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        var random = new Random();
        NativeList<Entity> resourceNoHolder = new NativeList<Entity>();
        Entities
            .WithBurst()
            .ForEach((Entity entity , in DropResourceComponent resource) =>
            {
                if (resource.holder == null)
                {
                    resourceNoHolder.Add(entity);
                }
            }).Schedule();

        Entities
            .WithName("DroneMove")
            .WithBurst()
            .ForEach((Entity entity, ref DroneComponent bee, ref Translation t, ref Rotation r, ref PhysicsVelocity pv, ref PhysicsMass pm) =>
            {
                //float3 impulse = -bee.Direction * bee.Magnitude;
                //impulse = math.rotate(r.Value, impulse);
                //impulse *= deltaTime;

                //float3 offset = math.rotate(r.Value, bee.Offset) + t.Value;

                //pv.ApplyImpulse(pm, t, r, impulse, offset);
                if (bee.resourceTarget == null)
                {
                    if (resourceNoHolder.Length > 0)
                    {
                        bee.resourceTarget = resourceNoHolder[random.NextInt(0, resourceNoHolder.Length)];
                    }
                }
                else if (bee.resourceTarget != null)
                {
                    //var resource = EntityManager.GetComponentData<DropResourceComponent>(bee.resourceTarget);
                    //var grabDistance = 1;
                    //var chaseForce = 1;
                    //if (resource.holder == null)
                    //{
                    //    {
                    //        var delta = resource.position - t.Value;
                    //        float sqrDist = delta.x * delta.x + delta.y * delta.y + delta.z * delta.z;
                    //        if (sqrDist > grabDistance * grabDistance)
                    //        {
                    //            pv.Linear += delta * (chaseForce * deltaTime / math.sqrt(sqrDist));
                    //        }
                    //        else if (resource.stacked)
                    //        {
                    //            GrabResource(entity, resource);
                    //        }
                    //    }
                    //}
                }
            }).Schedule();

        resourceNoHolder.Dispose();
    }

    public static void GrabResource(Entity bee, DropResourceComponent resource)
    {
        resource.holder = bee;
        resource.stacked = false;
    }
}
