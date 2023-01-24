using System.Collections.Generic;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Rendering;
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
        var ecb = new EntityCommandBuffer(Allocator.TempJob);
        for (int i = 1; i < uniques.Count; i++)
        {
            var setting = uniques[i];
            var resourceSize = uniques[i].resourceSize;
            var carryStiffness = uniques[i].carryStiffness;

            Entities
                .WithoutBurst()
                .WithSharedComponentFilter(setting)
                .ForEach((Entity e, ref ResourceItem resource) =>
                {
                    //接受资源处理的地方

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
                .WithSharedComponentFilter(setting)
                .ForEach((ref LocalToWorld localToWorld, in ResourceItem resource) =>
                {
                    float3 scale = new float3(resourceSize, resourceSize * .5f, resourceSize);
                    localToWorld = new LocalToWorld
                    {
                        Value = float4x4.TRS(resource.position, quaternion.identity, scale)
                    };
                }).Schedule();
        }


        Dependency.Complete();
        ecb.Playback(this.EntityManager);
        ecb.Dispose();
    }
}
[UpdateAfter(typeof(ResourceItemSystem))]
partial class CollectResourceSystem : SystemBase
{
    EntityQuery _collecterQuery;
    EntityCommandBufferSystem _endEcbSys;
    protected override void OnCreate()
    {
        _collecterQuery = GetEntityQuery(ComponentType.ReadOnly<ResourceCollection>());
        _endEcbSys = World.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    protected override void OnUpdate()
    {
        var ecb = _endEcbSys.CreateCommandBuffer();
        var allResourceCollecter = _collecterQuery.ToEntityArray(Allocator.TempJob);
        NativeList<FixedString128Bytes> itemIds = new NativeList<FixedString128Bytes>(Allocator.TempJob);
        Entities
            .WithoutBurst()
            .ForEach((Entity e, ref ResourceItem resource) =>
            {
                if (resource.holder == Entity.Null && !resource.dead)//等待释放
                {
                    //判断是否落入目标 TODO 值判断chunck中的减少计算
                    for (int i = 0; i < allResourceCollecter.Length; i++)
                    {
                        var collecterEntity = allResourceCollecter[i];
                        var colecterPosition = GetComponent<LocalToWorld>(collecterEntity).Position;
                        var collecterRange = 1 * 1;
                        if (math.distancesq(resource.position, colecterPosition) < collecterRange)
                        {
                            //ecb.RemoveComponent<ResourceItem>(e);bug多线程不同步 dronetarget 中出现找不到组件
                            resource.dead = true;
                            ecb.AddComponent(e, new LifeTime { Value = 1 });
                            //debug
                            var viewChild = GetBuffer<Child>(collecterEntity)[0].Value;
                            var tween = new TweenData(TypeOfTween.HdrColor, viewChild, UnityEngine.Color.black.ToFloat4(), 0.1f)
                                .SetEase(DG.Tweening.Ease.Linear)
                                .FromValue(UnityEngine.Color.white.ToFloat4());
                            TweenCreateSystem.AddTweenComponent<TweenHDRColorComponent>(ecb, tween);
                            itemIds.Add(GetComponent<WorldItem>(e).itemGuid);
                        }
                    }
                }
            }).Schedule();
        Dependency.Complete();
        Dependency = allResourceCollecter.Dispose(Dependency);

        _endEcbSys.AddJobHandleForProducer(Dependency);

        for (int i = 0; i < itemIds.Length; i++)
        {
            var guid = itemIds[i];
            InventoryManager.Instance.AddItem(guid.ToString());
        }
        Dependency = itemIds.Dispose(Dependency);
    }
}
