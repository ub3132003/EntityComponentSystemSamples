using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

namespace Steer
{
    [GenerateAuthoringComponent]

    public struct SteerForPoint : IComponentData, ISteering
    {
        /// <summary>
        /// Should the vehicle's velocity be considered in the seek calculations?
        /// </summary>
        /// <remarks>
        /// If true, the vehicle will slow down as it approaches its target. See
        /// the remarks on GetSeekVector.
        /// </remarks>
        public bool ConsiderVelocity; //set defulat false

        /// <summary>
        /// Should the target default to the vehicle current position if it's set to Vector3.zero?
        /// </summary>
        public bool _defaultToCurrentPosition;


        /// <summary>
        /// The target point.
        /// </summary>
        public float3 TargetPoint;

        public float Weight;

        public float3 weightForce;
        public float3 WeightForce { get { return weightForce; } set { weightForce = value; } }

        public float3 CalculateForce()
        {
            return float3.zero;
        }
    }
}
