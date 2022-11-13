using Unity.Entities;
using Unity.Assertions;

namespace Unity.Physics.Stateful
{
    // Trigger Event that can be stored inside a DynamicBuffer
    public struct StatefulTriggerEvent : IBufferElementData, IStatefulSimulationEvent<StatefulTriggerEvent>
    {
        internal EntityPair Entities;
        internal BodyIndexPair BodyIndices;
        internal ColliderKeyPair ColliderKeys;

        public StatefulEventState State { get; set; }
        public Entity EntityA => Entities.EntityA;
        public Entity EntityB => Entities.EntityB;
        public int BodyIndexA => BodyIndices.BodyIndexA;
        public int BodyIndexB => BodyIndices.BodyIndexB;
        public ColliderKey ColliderKeyA => ColliderKeys.ColliderKeyA;
        public ColliderKey ColliderKeyB => ColliderKeys.ColliderKeyB;

        public StatefulTriggerEvent(Entity entityA, Entity entityB, int bodyIndexA, int bodyIndexB,
                                    ColliderKey colliderKeyA, ColliderKey colliderKeyB)
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
            State = default;
        }

        public StatefulTriggerEvent(TriggerEvent triggerEvent)
        {
            Entities = new EntityPair
            {
                EntityA = triggerEvent.EntityA,
                EntityB = triggerEvent.EntityB
            };
            BodyIndices = new BodyIndexPair
            {
                BodyIndexA = triggerEvent.BodyIndexA,
                BodyIndexB = triggerEvent.BodyIndexB
            };
            ColliderKeys = new ColliderKeyPair
            {
                ColliderKeyA = triggerEvent.ColliderKeyA,
                ColliderKeyB = triggerEvent.ColliderKeyB
            };
            State = default;
        }

        // Returns other entity in EntityPair, if provided with one
        public Entity GetOtherEntity(Entity entity)
        {
            Assert.IsTrue((entity == EntityA) || (entity == EntityB));
            return (entity == EntityA) ? EntityB : EntityA;
        }

        public int CompareTo(StatefulTriggerEvent other) => ISimulationEventUtilities.CompareEvents(this, other);
    }
}
