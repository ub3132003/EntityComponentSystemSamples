using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Steer
{
    public interface ISteering
    {

        /// <summary>
        /// Force vector modified by the assigned weight 
        /// </summary>
        public float3 WeightForce
        {
            get; set;
        }
 
    }
}
