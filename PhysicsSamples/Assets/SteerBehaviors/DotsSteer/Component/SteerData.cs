using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
namespace Steer
{
    [GenerateAuthoringComponent]
    public struct SteerData : IComponentData
    {
        /// <summary>
        /// CalculateForce
        /// </summary>
        public float3 Force;
    }
}
