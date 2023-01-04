//using Unity.Burst;
//using Unity.Collections;
//using Unity.Entities;
//using Unity.Jobs;
//using Unity.Mathematics;
//using Unity.Transforms;
//using Unity.Physics.Stateful;
//using Unity.Physics;
//using UnityEngine;

//public partial class SwingBackSystem : SystemBase
//{

//    protected override void OnUpdate()
//    {
//        //var leftPress = Input.GetMouseButtonDown(0);
//        //if (lastPress != leftPress)
//        //{
//        //    lastPress = leftPress;
//        //    Entities
//        //        .WithStructuralChanges()
//        //        .ForEach((Entity e, in SwingBack swingBack) =>
//        //        {
//        //            EntityManager.SetEnabled(e, leftPress);
//        //            Debug.Log($"swingback enable : {leftPress}");
//        //        }).Run();
//        //}

//        Entities.ForEach((Entity e, ref Rotation rot, in DynamicBuffer<StatefulTriggerEvent> triggerBuffers, in SwingBack swingBack) => {
//            var ownTransfrom = GetComponent<LocalToWorld>(swingBack.OwnEntity);
//            rot.Value = quaternion.LookRotation(ownTransfrom.Forward, math.up());

//            for (int i = 0; i < triggerBuffers.Length; i++)
//            {
//                var triggerEvent = triggerBuffers[i];
//                if (triggerEvent.State == StatefulEventState.Stay)
//                {
//                    var enterEntity =  triggerEvent.GetOtherEntity(e);
//                    if (HasComponent<BulletComponent>(enterEntity))
//                    {
//                        var ballPv = GetComponent<PhysicsVelocity>(enterEntity);
//                        var targetVelocity = math.length(ballPv.Linear) * ownTransfrom.Forward;

//                        SetComponent(enterEntity, new PhysicsVelocity
//                        {
//                            Linear = targetVelocity,
//                            Angular = 0
//                        });
//                    }
//                }
//            }
//        }).Schedule();
//    }
//}
