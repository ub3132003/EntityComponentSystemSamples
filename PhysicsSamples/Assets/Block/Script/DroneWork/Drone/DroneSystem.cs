using Unity.Entities;
using Unity.Mathematics;
using Unity.Rendering;
using Unity.Transforms;

using System.Collections.Generic;
using Latios;
using Unity.Collections;

[UpdateInGroup(typeof(SimulationSystemGroup))]
[UpdateBefore(typeof(ResourceItemSystem))]
partial class DroneSystem : SystemBase
{
    public struct Field
    {
        public float3 size;
        public float gravity;
    }
    Rng m_rng;
    EntityQuery resourceQuery;
    protected override void OnCreate()
    {
        m_rng = new Rng("DroneSystem");
        resourceQuery = GetEntityQuery(typeof(ResourceItem));
    }

    /// <summary>
    /// 返回随机的资源实体,如果资源已经被持有则返回空
    /// </summary>
    /// <returns></returns>
    static Entity TryGetRandomResource(NativeArray<Entity> resources , Rng.RngSequence random)
    {
        var count = resources.Length;
        if (count == 0)
        {
            return Entity.Null;
        }
        else
        {
            Entity resource = resources[random.NextInt(0, count)];
            //int stackHeight = instance.stackHeights[resource.gridX, resource.gridY];
            return resource;
        }
    }

    protected override void OnUpdate()
    {
        Field field = new Field
        {
            size = new float3(100f, 20f, 30f),
            gravity = -9.8f
        };

        float deltaTime = Time.fixedDeltaTime;
        var rng = m_rng;
        var gravity = -9.8f;

        List<DroneSettings> uniques = new List<DroneSettings>();
        EntityManager.GetAllUniqueSharedComponentData(uniques);
        EntityCommandBuffer ecb = new EntityCommandBuffer(Allocator.TempJob);
        for (int i = 0; i < uniques.Count; i++)
        {
            var droneSetting = uniques[i];
            var speedStretch = droneSetting.speedStretch;
            var flightJitter = droneSetting.flightJitter;
            var damping = droneSetting.damping;
            var rotationStiffness = droneSetting.rotationStiffness;
            var chaseForce = droneSetting.chaseForce;
            var carryForce = droneSetting.carryForce;
            var grabDistance = droneSetting.grabDistance;

            var resourceItems = resourceQuery.ToEntityArray(Unity.Collections.Allocator.TempJob);
            if (resourceItems.Length == 0) continue;
            var teamsOfBees = GetComponentDataFromEntity<Drone>(true);
            var world = World.Unmanaged;
            var resourceHolderTeam = CollectionHelper.CreateNativeArray<int, RewindableAllocator>(resourceItems.Length, ref world.UpdateAllocator);
            var freeResources = CollectionHelper.CreateNativeArray<ResourceItem, RewindableAllocator>(resourceItems.Length, ref world.UpdateAllocator);
            var freeResourcesEntity = new NativeList<Entity>(resourceItems.Length, Allocator.TempJob);
            var findFreeResourceJobHanlde = Entities
                .WithoutBurst()
                .WithName("FindFreeResource")
                .ForEach((Entity e, in ResourceItem resource) =>
                {
                    if (resource.holder == Entity.Null)
                    {
                        freeResourcesEntity.Add(e);
                    }
                }).Schedule(Dependency);
            findFreeResourceJobHanlde.Complete();

            // 寻找空闲的目标
            var droneWorkJobHandle = Entities
                .WithoutBurst()
                .WithName("DroneWork")
                .WithReadOnly(freeResourcesEntity)
                .ForEach((int entityInQueryIndex, Entity e,  ref Drone bee) =>
                {
                    var random = rng.GetSequence(entityInQueryIndex);
                    //Bee bee = bees[i];
                    //bee.isAttacking = false;
                    //bee.isHoldingResource = false;
                    //todo life dead 判断
                    bee.velocity += random.NextFloat3Direction() * random.NextFloat(0, 1) * (flightJitter * deltaTime);
                    bee.velocity *= (1f - damping);

                    float3 delta;
                    float dist;
                    //
                    bee.velocity.y += gravity * deltaTime;
                    //bee.deathTimer -= deltaTime / 10f;
                    //if (bee.deathTimer < 0f)
                    //{
                    //    DeleteBee(bee);
                    //}
                    //资源获取寻找,如果没有持有的时候
                    if (bee.resourceTarget == Entity.Null)
                    {
                        //必定是无主
                        if (freeResourcesEntity.Length > 0)
                        {
                            var resourceIndex = random.NextInt(0, freeResourcesEntity.Length);
                            bee.resourceTarget = freeResourcesEntity[resourceIndex];
                        }
                    }
                    //当有目标时
                    else
                    {
                        var resourceTarget = GetComponent<ResourceItem>(bee.resourceTarget);
                        //检查目标资源时候已经被其他蜜蜂抓走,重新选择目标
                        if (resourceTarget.hasHolder && resourceTarget.holder != e)//todu 敌对抢夺
                        {
                            bee.resourceTarget = Entity.Null;
                        }
                        else if (resourceTarget.holder == e)
                        {
                            //搬运放到目标home区域
                            float3 targetPos = float3.zero;
                            delta = targetPos - bee.position;
                            dist = math.length(delta);
                            bee.velocity += (targetPos - bee.position) * (carryForce * deltaTime / dist);
                            if (dist < 1f)
                            {
                                resourceTarget.ClearHolder();
                                ecb.SetComponent(bee.resourceTarget, resourceTarget);
                                bee.ClearResource();
                            }
                        }
                        else
                        {
                            //向目标移动
                            delta = resourceTarget.position - bee.position;
                            float sqrDist = math.lengthsq(delta);
                            if (sqrDist > grabDistance * grabDistance)
                            {
                                bee.velocity += delta * (chaseForce * deltaTime / math.sqrt(sqrDist));
                            }
                            else//进入抓取范围
                            {
                                resourceTarget.GrabResource(e);
                                ecb.SetComponent(bee.resourceTarget, resourceTarget);
                                bee.SetReource();
                            }
                        }


                        //抢夺资源
                        //else if (resource.holder.team != bee.team)
                        //{
                        //    bee.enemyTarget = resource.holder;
                        //}
                        //else if (resourceTarget.holderTeam == bee.team)
                        //{
                        //    bee.resourceTarget = Entity.Null;
                        //}
                    }

                    bee.position += deltaTime * bee.velocity;

                    //防止偏离中心区域
                    if (System.Math.Abs(bee.position.x) > field.size.x * .5f)
                    {
                        bee.position.x = (field.size.x * .5f) * math.sign(bee.position.x);
                        bee.velocity.x *= -.5f;
                        bee.velocity.y *= .8f;
                        bee.velocity.z *= .8f;
                    }
                    if (System.Math.Abs(bee.position.z) > field.size.z * .5f)
                    {
                        bee.position.z = (field.size.z * .5f) * math.sign(bee.position.z);
                        bee.velocity.z *= -.5f;
                        bee.velocity.x *= .8f;
                        bee.velocity.y *= .8f;
                    }
                    float resourceModifier = 0f;
                    if (bee.isHoldingResource)
                    {
                        resourceModifier = 0.75f;//source size
                    }
                    if (System.Math.Abs(bee.position.y) > field.size.y * .5f - resourceModifier)
                    {
                        bee.position.y = (field.size.y * .5f - resourceModifier) * math.sign(bee.position.y);
                        bee.velocity.y *= -.5f;
                        bee.velocity.z *= .8f;
                        bee.velocity.x *= .8f;
                    }
                    // only used for smooth rotation:
                    float3 oldSmoothPos = bee.smoothPosition;
                    if (bee.isAttacking == false)
                    {
                        bee.smoothPosition = math.lerp(bee.smoothPosition, bee.position, deltaTime * rotationStiffness);
                    }
                    else
                    {
                        bee.smoothPosition = bee.position;
                    }
                    bee.smoothDirection = bee.smoothPosition - oldSmoothPos;
                }).Schedule(findFreeResourceJobHanlde);
            rng = m_rng.Shuffle();

            //output 最终结果输出到画面
            var finalDroneJobHandle = Entities
                .ForEach((ref LocalToWorld localToWorld, in Drone bee) =>
            {
                float size = bee.size;
                float3 scale = new float3(size, size, size);
                if (bee.dead == false)
                {
                    float stretch = math.max(1f, math.length(bee.velocity) * speedStretch);
                    scale.z *= stretch;
                    scale.x /= (stretch - 1f) / 5f + 1f;
                    scale.y /= (stretch - 1f) / 5f + 1f;
                }
                quaternion rotation = quaternion.identity;
                if (!bee.smoothDirection.IsZero())
                {
                    rotation = quaternion.LookRotation(bee.smoothDirection, math.up());
                }

                localToWorld = new LocalToWorld
                {
                    Value = float4x4.TRS(bee.position, rotation, scale)
                };
            }).Schedule(droneWorkJobHandle);
            Dependency = finalDroneJobHandle;
            //死蜜蜂过度动画
            Entities
                .ForEach((in Drone bee) =>
            {
                if (bee.dead)
                {
                    //color *= .75f;
                    //scale *= math.sqrt(bee.deathTimer);
                }
            }).Schedule();

            Dependency.Complete();

            resourceItems.Dispose();
            freeResourcesEntity.Dispose();
            resourceQuery.AddDependency(Dependency);
            resourceQuery.ResetFilter();
        }
        ecb.Playback(EntityManager);
        ecb.Dispose();
        uniques.Clear();
    }
}
