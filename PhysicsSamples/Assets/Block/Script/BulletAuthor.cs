using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;
using System;

public struct BulletComponent : IComponentData
{
    /// <summary>
    /// 预制体的id
    /// </summary>
    public int BulletPerfab;
    public float2 SpeedRange;
    public float3 LockAixs;
}

[DisallowMultipleComponent]
public class BulletAuthor : MonoBehaviour, IConvertGameObjectToEntity
{
    public RpgEffectSO effectSO;
    public int rank;

    [Min(0)]
    public int Damage;
    public COST_TYPES Type;
    public ElementType DamageElementType;
    public float2 SpeedRange;
    public float3 LockAixs;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //不由gun 实例化的子弹
        if (!dstManager.HasComponent<BulletComponent>(entity))
        {
            dstManager.AddComponentData(entity, new BulletComponent());
        }
        var bullet = dstManager.GetComponentData<BulletComponent>(entity);
        bullet.BulletPerfab = gameObject.GetInstanceID();
        bullet.SpeedRange = SpeedRange;
        bullet.LockAixs = LockAixs;
        dstManager.SetComponentData(entity, bullet);

        dstManager.AddBuffer<StatefulCollisionEvent>(entity);
        dstManager.AddComponentData(entity, new Damage
        {
            DamageValue = Damage,
            Type = Type,
            DamageElementType = DamageElementType,
        });
    }
}

//[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
//public partial class TriggerBulletSystem : SystemBase
//{
//    protected override void OnUpdate()
//    {
//        Entities //BUG 改变速度后反弹角度不正确
//            .WithName("KeepBullet")
//            .WithoutBurst()
//            .ForEach((ref BulletComponent bullet, ref Translation t, ref Rotation r, ref PhysicsVelocity pv, ref PhysicsMass pm) =>
//            {
//                pv.Linear = math.normalizesafe(pv.Linear) * 30;
//                pv.Angular = 0;
//                //pv.Linear = math.normalize(pv.Linear) * 35;
//            }).Schedule();
//    }
//}
