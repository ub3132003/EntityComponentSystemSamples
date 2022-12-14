using Steer;
using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace Stree
{
    public struct BipedMove : IComponentData, IVehicle
    {
        public float3 Velocity;
        public float Speed { get; set; }

        public float3 DesiredVelocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public float TargetSpeed { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
        public bool CanMove { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

        float3 IVehicle.Velocity { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }
    }
}
