using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class ResourceItemSystem : SystemBase
{
    public struct Field
    {
        public float3 size;
        public float gravity;
    }

    protected override void OnUpdate()
    {
        Field field = new Field
        {
            size = new float3(100f, 20f, 30f),
            gravity = -9.8f
        };
        var deltaTime = Time.DeltaTime;
        List<ResourceItemSetting> uniques = new List<ResourceItemSetting>();
        EntityManager.GetAllUniqueSharedComponentData(uniques);

        for (int i = 0; i < uniques.Count; i++)
        {
            var resourceSize = uniques[i].resourceSize;
            var carryStiffness = uniques[i].carryStiffness;

            Entities
                .ForEach((ref ResourceItem resource) =>
            {
                if (resource.holder != Entity.Null)
                {
                    var holder = GetComponent<Drone>(resource.holder);
                    if (holder.dead)
                    {
                        resource.holder = Entity.Null;
                    }
                    else
                    {
                        float3 targetPos = holder.position - math.up() * (resourceSize + holder.size) * .5f;
                        resource.position = math.lerp(resource.position, targetPos, carryStiffness * deltaTime);
                        resource.velocity = holder.velocity;
                    }
                }
                else if (resource.stacked == false)
                {
                    //resource.position = math.lerp(resource.position, NearestSnappedPos(resource.position), snapStiffness * Time.deltaTime);
                    resource.velocity.y += field.gravity * deltaTime;
                    resource.position += resource.velocity * deltaTime;
                    //GetGridIndex(resource.position, out resource.gridX, out resource.gridY);
                    //float floorY = GetStackPos(resource.gridX, resource.gridY, stackHeights[resource.gridX, resource.gridY]).y;
                    for (int j = 0; j < 3; j++)
                    {
                        if (System.Math.Abs(resource.position[j]) > field.size[j] * .5f)
                        {
                            resource.position[j] = field.size[j] * .5f * math.sign(resource.position[j]);
                            resource.velocity[j] *= -.5f;
                            resource.velocity[(j + 1) % 3] *= .8f;
                            resource.velocity[(j + 2) % 3] *= .8f;
                        }
                    }
                }
            }).Schedule();

            Entities
                .ForEach((ref LocalToWorld localToWorld, in ResourceItem resource) =>
            {
                float3 scale = new float3(resourceSize, resourceSize * .5f, resourceSize);
                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(resource.position, quaternion.identity, scale)
                };
            }).Schedule();
        }
    }

    public static void GrabResource(Entity bee, ref ResourceItem resource)
    {
        resource.holder = bee;
        resource.stacked = false;
    }
}
