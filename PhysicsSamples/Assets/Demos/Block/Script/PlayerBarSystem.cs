using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using Unity.Physics.Stateful;
using UnityEngine;

[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(BlockHitSystem))]
public partial class PlayerBarSystem : SystemBase
{
    protected override void OnUpdate()
    {
        //玩家回击子弹
        Entities
            .WithName("BarBeatBall")
            .WithAll<FollowMouseOnGroud>()
            .ForEach((Entity playerBar,  ref DynamicBuffer<StatefulCollisionEvent> collisionBuff , in FollowMouseOnGroud plyerMove) =>
            {
                var length = collisionBuff.Length;
                for (int i = 0; i < length; i++)
                {
                    var collisionEvent = collisionBuff[i];
                    var bullet = collisionEvent.GetOtherEntity(playerBar);

                    if (collisionEvent.State != StatefulEventState.Enter || !HasComponent<BulletComponent>(bullet))
                    {
                        continue;
                    }
                    var bulletPv = GetComponent<PhysicsVelocity>(bullet);
                    Debug.Log($"b:{bulletPv.Linear}  p:{GetComponent<PhysicsVelocity>(playerBar).Linear * plyerMove.HitForce}");
                    bulletPv.Linear += GetComponent<PhysicsVelocity>(playerBar).Linear* plyerMove.HitForce;
                    SetComponent(bullet, new PhysicsVelocity
                    {
                        Linear = bulletPv.Linear
                    });
                }
            }).Schedule();

        //子弹速度限制
        Entities
            .ForEach((ref PhysicsVelocity bulletPv, in BulletComponent bullet) =>
        {
            var dir = math.normalizesafe(bulletPv.Linear);
            //限制反弹速度
            var speed = math.clamp(math.length(bulletPv.Linear), bullet.SpeedRange.x, bullet.SpeedRange.y);
            bulletPv.Linear = dir * speed * bullet.LockAixs;
            //Debug.Log($"next:{bulletPv.Linear}");
        }).Schedule();
    }
}
