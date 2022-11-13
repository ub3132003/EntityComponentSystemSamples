using System;
using Unity.Assertions;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Physics.Systems;
using UnityEngine;

namespace Unity.Physics.Stateful
{
    public class DynamicBufferCollisionEventAuthoring : MonoBehaviour, IConvertGameObjectToEntity
    {
        [Tooltip("If selected, the details will be calculated in collision event dynamic buffer of this entity")]
        public bool CalculateDetails = false;

        public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            var dynamicBufferTag = new StatefulCollisionEventDetails
            {
                CalculateDetails = CalculateDetails ? true : false
            };

            dstManager.AddComponentData(entity, dynamicBufferTag);
            dstManager.AddBuffer<StatefulCollisionEvent>(entity);
        }
    }
}
