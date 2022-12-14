using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Rival;

using Unity.Physics.Extensions;
using Unity.Physics;

using UnityEngine;
using Unity.Physics.Systems;

[UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
[UpdateAfter(typeof(TopDownPlayerSystem))]
[UpdateBefore(typeof(FixedStepSimulationSystemGroup))]
[UpdateBefore(typeof(TransformSystemGroup))]
public partial class TopDownCharacterRotationSystem : SystemBase
{
    public FixedStepSimulationSystemGroup FixedStepSimulationSystemGroup;
    public EntityQuery CharacterQuery;
    private BuildPhysicsWorld m_BuildPhysicsWorld;
    protected override void OnCreate()
    {
        base.OnCreate();

        FixedStepSimulationSystemGroup = World.GetOrCreateSystem<FixedStepSimulationSystemGroup>();

        CharacterQuery = GetEntityQuery(new EntityQueryDesc
        {
            All = MiscUtilities.CombineArrays(
                KinematicCharacterUtilities.GetCoreCharacterComponentTypes(),
                new ComponentType[]
                {
                    typeof(TopDownCharacterComponent),
                    typeof(TopDownCharacterInputs),
                }),
        });

        m_BuildPhysicsWorld = World.GetOrCreateSystem<BuildPhysicsWorld>();

        RequireForUpdate(CharacterQuery);
    }

    protected override void OnUpdate()
    {
        float deltaTime = Time.DeltaTime;
        float fixedDeltaTime = FixedStepSimulationSystemGroup.RateManager.Timestep;

        var collisionWorld = m_BuildPhysicsWorld.PhysicsWorld.CollisionWorld;
        Vector2 mousePosition = Input.mousePosition;
        UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(mousePosition);
        var rayInput = new RaycastInput
        {
            Start = unityRay.origin,
            End = unityRay.origin + unityRay.direction * MousePickSystem.k_MaxDistance,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = ~(1u << 3),
                GroupIndex = 0
            }
        };


        Unity.Physics.RaycastHit hit = default;

        if (collisionWorld.CastRay(rayInput, out hit))
        {
            Debug.DrawLine(Camera.main.transform.position, hit.Position, Color.red);
        }
        else { return; }

        var hitPos = hit.Position;

        Entities.ForEach((
            Entity entity,
            ref Rotation characterRotation,
            ref TopDownCharacterComponent character,
            in TopDownCharacterInputs characterInputs,
            in KinematicCharacterBody characterBody,
            in Translation translation) =>
            {
                var dir = hitPos - translation.Value;
                dir.y = 0;
                characterRotation.Value = quaternion.LookRotation(dir, math.up());


                // Add rotation from parent body to the character rotation
                // (this is for allowing a rotating moving platform to rotate your character as well, and handle interpolation properly)
                KinematicCharacterUtilities.ApplyParentRotationToTargetRotation(ref characterRotation.Value, in characterBody, fixedDeltaTime, deltaTime);
            }).Schedule();
    }
}
