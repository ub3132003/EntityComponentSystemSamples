using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using System.Collections;
using Steer;

namespace Stree
{
    public struct AutonomousVehicle : IComponentData, IVehicle
    {
        public float _accelerationRate;//5

        public float _decelerationRate;//8

        public float3 DesiredVelocity { get; set; }
        public float InverseMass;
        public float Speed { get; set; }

        public float3 Velocity { get; set; }
        public float TargetSpeed { get; set; }
        public bool CanMove { get; set; }
    }
}
