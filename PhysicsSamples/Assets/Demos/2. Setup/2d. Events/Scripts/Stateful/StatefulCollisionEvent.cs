using Unity.Assertions;
using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Physics.Stateful
{
    // Collision Event that can be stored inside a DynamicBuffer
    public struct StatefulCollisionEvent : IBufferElementData, IStatefulSimulationEvent<StatefulCollisionEvent>
    {
        internal BodyIndexPair BodyIndices;
        internal EntityPair Entities;
        internal ColliderKeyPair ColliderKeys;

        // Only if CalculateDetails is checked on PhysicsCollisionEventBuffer of selected entity,
        // this field will have valid value, otherwise it will be zero initialized
        public Details CollisionDetails;

        public StatefulEventState State { get; set; }

        // Normal is pointing from EntityB to EntityA
        public float3 Normal;

        public StatefulCollisionEvent(Entity entityA, Entity entityB, int bodyIndexA, int bodyIndexB,
                                      ColliderKey colliderKeyA, ColliderKey colliderKeyB, float3 normal)
        {
            Entities = new EntityPair
            {
                EntityA = entityA,
                EntityB = entityB
            };
            BodyIndices = new BodyIndexPair
            {
                BodyIndexA = bodyIndexA,
                BodyIndexB = bodyIndexB
            };
            ColliderKeys = new ColliderKeyPair
            {
                ColliderKeyA = colliderKeyA,
                ColliderKeyB = colliderKeyB
            };
            Normal = normal;
            State = default;
            CollisionDetails = default;
        }

        public Entity EntityA => Entities.EntityA;
        public Entity EntityB => Entities.EntityB;
        public ColliderKey ColliderKeyA => ColliderKeys.ColliderKeyA;
        public ColliderKey ColliderKeyB => ColliderKeys.ColliderKeyB;
        public int BodyIndexA => BodyIndices.BodyIndexA;
        public int BodyIndexB => BodyIndices.BodyIndexB;

        public StatefulCollisionEvent(CollisionEvent collisionEvent)
        {
            Entities = new EntityPair
            {
                EntityA = collisionEvent.EntityA,
                EntityB = collisionEvent.EntityB
            };
            BodyIndices = new BodyIndexPair
            {
                BodyIndexA = collisionEvent.BodyIndexA,
                BodyIndexB = collisionEvent.BodyIndexB
            };
            ColliderKeys = new ColliderKeyPair
            {
                ColliderKeyA = collisionEvent.ColliderKeyA,
                ColliderKeyB = collisionEvent.ColliderKeyB
            };

            State = default;
            Normal = collisionEvent.Normal;
            CollisionDetails = default;
        }

        // This struct describes additional, optional, details about collision of 2 bodies
        public struct Details
        {
            public bool IsValid;

            // If 1, then it is a vertex collision
            // If 2, then it is an edge collision
            // If 3 or more, then it is a face collision
            public int NumberOfContactPoints;

            // Estimated impulse applied
            public float EstimatedImpulse;
            // Average contact point position
            public float3 AverageContactPointPosition;

            public Details(int numContactPoints, float estimatedImpulse, float3 averageContactPosition)
            {
                IsValid = (0 < numContactPoints); // Should we add a max check?
                NumberOfContactPoints = numContactPoints;
                EstimatedImpulse = estimatedImpulse;
                AverageContactPointPosition = averageContactPosition;
            }
        }

        // Returns the other entity in EntityPair, if provided with other one
        public Entity GetOtherEntity(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            return entity == EntityA ? EntityB : EntityA;
        }

        // Returns the normal pointing from passed entity to the other one in pair
        public float3 GetNormalFrom(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            return math.select(-Normal, Normal, entity == EntityB);
        }

        public bool TryGetDetails(out Details details)
        {
            details = CollisionDetails;
            return CollisionDetails.IsValid;
        }

        public int CompareTo(StatefulCollisionEvent other) => ISimulationEventUtilities.CompareEvents(this, other);
    }
}
