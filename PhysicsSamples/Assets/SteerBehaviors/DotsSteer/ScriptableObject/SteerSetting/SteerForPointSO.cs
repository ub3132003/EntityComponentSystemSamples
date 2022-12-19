using Sirenix.OdinInspector;
using Unity.Entities;
using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Steer
{
    public class SteerForPointSO : SteerSettingSO , IConvertToComponentData<SteerForPoint>
    {
        public bool ConsiderVelocity; //set defulat false

        public bool _defaultToCurrentPosition;


        /// <summary>
        /// The target point.
        /// </summary>
        public float3 TargetPoint;
        public override void AddComponentData(Entity entity, EntityManager dstManager)
        {
            dstManager.AddComponentData(entity, ToComponent());
        }

        public SteerForPoint ToComponent()
        {
            return
                new SteerForPoint
                {
                    ConsiderVelocity = ConsiderVelocity,
                    Weight = Weight,
                };
        }
    }
}
