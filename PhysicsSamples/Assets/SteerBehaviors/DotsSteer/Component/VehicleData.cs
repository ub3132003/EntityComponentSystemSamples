using Steer;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UIElements;

[GenerateAuthoringComponent]
public struct VehicleData : IComponentData
{
    /// <summary>
    /// Value of return CalculatePositionDelta
    /// </summary>
    public float3 Acceleration;

    /// <summary>
    /// Current magnitude for the vehicle's velocity.
    /// </summary>
    /// <remarks>
    /// It is expected to be set at the same time that the Velocity is
    /// assigned in one of the descendent classes.  It may or may not
    /// match the vehicle speed, depending on how that is calculated -
    /// for example, some subclasses can use a Speedometer to calculate
    /// their speed.
    /// </remarks>
    public float TargetSpeed;

    /// <summary>
    /// Velocity vector used to orient the agent.
    /// </summary>
    /// <remarks>
    /// This is expected to be set by the subclasses.
    /// </remarks>
    public float3 OrientationVelocity;

    /// <summary>
    /// Current vehicle velocity. Subclasses are likely to only actually
    /// implement one of the two methods.
    /// </summary>
    public float3 Velocity;

    /// <summary>
    /// Current vehicle speed
    /// </summary>
    public float Speed;

    /// <summary>
    /// The velocity desired by this vehicle, likely calculated by means
    /// similar to what AutonomousVehicle does
    /// </summary>
    public float3 DesiredVelocity;

    /// <summary>
    /// Enforce speed limit.  Steering behaviors are expected to return a
    /// final desired velocity, not a acceleration, so we apply them directly.
    /// </summary>
    public float3 NewVelocity;

    /// <summary>
    /// sum of steer force
    /// </summary>
    public float3 Force;
    /// <summary>
    /// sum of pass-steer force
    /// </summary>
    public float3 passForce;

    //moveData
    public float3 GetSeekVector(VehicleSharedData type, float3 target, float3 selfPosition, bool considerVelocity = false)
    {
        float3 force = float3.zero;
        var difference = target - selfPosition;// translation.Value;
        var d = math.lengthsq(difference);
        var arrivalRadius = type.ArrivalRadius;
        if (d > arrivalRadius * arrivalRadius)
        {
            /* But suppose we still have some distance to go. The first step
            * then would be calculating the steering force necessary to orient
            * ourselves to and walk to that point.
            *
            * It doesn't apply the steering itself, simply returns the value so
            * we can continue operating on it.
            */
            force = considerVelocity ? difference - Velocity : difference;
        }
        return force;
    }
}
