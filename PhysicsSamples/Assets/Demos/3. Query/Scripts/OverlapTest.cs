using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using Unity.Transforms;
using UnityEngine;
/// <summary>
/// 定时对范围内检测并施加力
/// </summary>
///
[GenerateAuthoringComponent]
public struct OverlapExplode : IComponentData
{
    public float3 Range;
    public float3 Force;
}

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//[UpdateAfter(typeof(StepPhysicsWorld))]
//[UpdateBefore(typeof(EndFramePhysicsSystem))]
public partial class OverlapTest : SystemBase
{
    private BuildPhysicsWorld m_BuildPhysicsWorld;
    protected override void OnCreate()
    {
        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();
    }

    protected override void OnUpdate()
    {
        var physicsWorld = m_BuildPhysicsWorld.PhysicsWorld;
        var distanceHits = new NativeList<DistanceHit>(8, Allocator.TempJob);
        var jobHandle =
            Entities.ForEach((ref OverlapExplode explode , in Translation t , in Rotation r) => {
                //physicsWorld.CollisionWorld.OverlapAabb(input, ref overlappingBodies);

                var filter = new CollisionFilter
                {
                    BelongsTo = ~0u,
                    CollidesWith = ~0u,
                    GroupIndex = 0
                };

                if (physicsWorld.CollisionWorld.OverlapBox(t.Value, r.Value, explode.Range, ref distanceHits, filter))
                {
                    //Debug.Log($"hit: {distanceHits[0].Entity}");
                    for (int i = 0; i < distanceHits.Length; i++)
                    {
                        var other = distanceHits[i].Entity;
                        if (HasComponent<PhysicsVelocity>(other))
                        {
                            var pv = GetComponent<PhysicsVelocity>(other);
                            var pm = GetComponent<PhysicsMass>(other);
                            var force = new float3(0, 10, 0);
                            var tOther = GetComponent<Translation>(other);
                            var rOther = GetComponent<Rotation>(other);
                            pv.ApplyImpulse(pm, tOther, rOther, force, distanceHits[i].Position);
                            //pv.Linear = math.up() * 3;
                            SetComponent(other, pv);
                        }
                    }
                }
            }).Schedule(Dependency);

        distanceHits.Dispose();
    }
}
