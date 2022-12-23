using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using UnityEngine;
using Unity.Physics;
using Unity.Transforms;
using Unity.Physics.Extensions;
using Unity.Physics.Systems;

public struct BulletComponent : IComponentData
{
    public int Damage;
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
        dstManager.AddComponentData(entity, new BulletComponent
        {
            Damage = Damage,
            SpeedRange = SpeedRange,
            LockAixs = LockAixs
        });
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
