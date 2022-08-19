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

                    var dir = math.normalize(bulletPv.Linear);
                    //限制反弹速度
                    var speed = math.clamp(math.length(bulletPv.Linear), 5, 30);
                    bulletPv.Linear = dir * speed;
                    Debug.Log($"next:{bulletPv.Linear}");
                    SetComponent(bullet, new PhysicsVelocity
                    {
                        Linear = bulletPv.Linear
                    });
                }
            }).Schedule();

        //子弹速度限制
        Entities
            .WithName("BulletSpeed")
            .WithNone<BulletRockTag>()
            .WithAll<BulletComponent>()
            .ForEach((ref PhysicsVelocity pv) =>
            {
                //限制y轴速度
                var velocity = pv.Linear;
                velocity.y = 0;
                pv.Linear = velocity;
            }).Schedule();
    }
}
