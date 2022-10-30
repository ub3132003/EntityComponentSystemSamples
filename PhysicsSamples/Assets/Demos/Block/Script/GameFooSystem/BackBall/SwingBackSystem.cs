using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics.Stateful;
using Unity.Physics;
public partial class SwingBackSystem : SystemBase
{
    protected override void OnUpdate()
    {
        Entities.ForEach((Entity e, in DynamicBuffer<StatefulTriggerEvent> triggerBuffers, in SwingBack swingBack , in LocalToWorld localToWorld) => {
            for (int i = 0; i < triggerBuffers.Length; i++)
            {
                var triggerEvent = triggerBuffers[i];
                if (triggerEvent.State == StatefulEventState.Enter)
                {
                    var enterEntity =  triggerEvent.GetOtherEntity(e);
                    if (HasComponent<BulletComponent>(enterEntity))
                    {
                        var ballPv = GetComponent<PhysicsVelocity>(enterEntity);
                        var targetVelocity = math.length(ballPv.Linear) * localToWorld.Forward;
                        SetComponent(enterEntity, new PhysicsVelocity
                        {
                            Linear = targetVelocity,
                            Angular = 0
                        });
                    }
                }
            }
        }).Schedule();
    }
}
