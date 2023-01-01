using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Physics;
using UnityEngine;
using UnityEngine.EventSystems;

public struct CharacterGun : IComponentData
{
    /// <summary>
    /// 子弹预制体id
    /// </summary>
    public int ID;
    public Entity Bullet;
    public float Strength;
    public float Rate;
    public float Duration;

    public int WasFiring;
    public int IsFiring;

    public float SensitivityYAxis;
    public float Price;
    public int Capacity;
    /// <summary>
    /// 子弹容量上限
    /// </summary>
    public int MaxCapcity;
}

public struct CharacterGunInput : IComponentData
{
    public float2 Looking;
    public float Firing;
}

public class CharacterGunAuthoring : MonoBehaviour, IDeclareReferencedPrefabs, IConvertGameObjectToEntity
{
    public GameObject Bullet;

    public float Strength = 10;
    public float Rate = 1;
    public float SensitivityYAxis = 0;
    /// <summary>
    /// 生成子弹需要的花费
    /// </summary>
    public float Price = 1f;
    /// <summary>
    /// 子弹容量
    /// </summary>
    public int Capacity = 10;
    public int MaxCapcity = 10;
    // Referenced prefabs have to be declared so that the conversion system knows about them ahead of time
    public void DeclareReferencedPrefabs(List<GameObject> gameObjects)
    {
        gameObjects.Add(Bullet);
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var gun =
            new CharacterGun
        {
            ID = Bullet.GetInstanceID(),
            Bullet = conversionSystem.GetPrimaryEntity(Bullet),
            Strength = Strength,
            Rate = Rate,
            WasFiring = 0,
            IsFiring = 0,
            SensitivityYAxis = SensitivityYAxis,
            Capacity = Capacity,
            MaxCapcity = MaxCapcity,
        };
        dstManager.AddComponentData(entity, gun);
        //if (!dstManager.HasComponent<BulletComponent>(gun.Bullet))
        //{
        //    var bullet = new BulletComponent
        //    {
        //        BulletPerfab = gun.Bullet.Index,
        //    };
        //    dstManager.AddComponentData(gun.Bullet, bullet);
        //}
        //else
        //{
        //    var bullet = dstManager.GetComponentData<BulletComponent>(gun.Bullet);
        //    bullet.BulletPerfab = gun.Bullet.Index;
        //    dstManager.SetComponentData(gun.Bullet, bullet);
        //}
    }
}

#region System
// Update before physics gets going so that we don't have hazard warnings.
// This assumes that all gun are being controlled from the same single input system
[UpdateInGroup(typeof(FixedStepSimulationSystemGroup))]
[UpdateAfter(typeof(CharacterControllerSystem))]
public partial class CharacterGunOneToManyInputSystem : SystemBase
{
    EntityCommandBufferSystem m_EntityCommandBufferSystem;

    protected override void OnCreate() =>
        m_EntityCommandBufferSystem = World.GetOrCreateSystem<EndFixedStepSimulationEntityCommandBufferSystem>();

    protected override void OnUpdate()
    {
        //操作ui时不发射
        //if (EventSystem.current == null || EventSystem.current.IsPointerOverGameObject())
        //{
        //    return;
        //}

        var commandBuffer = m_EntityCommandBufferSystem.CreateCommandBuffer().AsParallelWriter();
        var input = GetSingleton<CharacterGunInput>();
        float dt = Time.DeltaTime;
        float comsumeSunCoin = 0f;
        Entities
            .WithName("CharacterControllerGunToManyInputJob")
            .WithBurst()
            .WithNone<DisableTag>()
            .ForEach((Entity entity, int entityInQueryIndex, ref Rotation gunRotation, ref CharacterGun gun, in LocalToWorld gunTransform) =>
            {
                //// Handle input , 输入控制发射球仰角
                //{
                //    float a = -input.Looking.y * gun.SensitivityYAxis;
                //    gunRotation.Value = math.mul(gunRotation.Value, quaternion.Euler(math.radians(a), 0, 0));
                //    gun.IsFiring = input.Firing > 0f ? 1 : 0;
                //}
                //长按计时
                //if (gun.IsFiring == 0)
                //{
                //    gun.Duration = 0;
                //    gun.WasFiring = 0;
                //    return;
                //}
                //if (gun.Capacity <= 0) { gun.IsFiring = 0; return; }
                gun.Duration += dt;
                if ((gun.Duration > gun.Rate) || (gun.WasFiring == 0))
                {
                    if (gun.Bullet != null)
                    {
                        var bullet = commandBuffer.Instantiate(entityInQueryIndex, gun.Bullet);

                        gun.Capacity--;
                        comsumeSunCoin += gun.Price;
                        Translation position = new Translation { Value = gunTransform.Position + gunTransform.Forward };
                        Rotation rotation = new Rotation { Value = gunRotation.Value };
                        PhysicsVelocity velocity = new PhysicsVelocity
                        {
                            Linear = gunTransform.Forward * gun.Strength,
                            Angular = float3.zero
                        };
                        if (HasComponent<CompositeScale>(gun.Bullet))
                        {
                            var compositeScale = GetComponent<CompositeScale>(gun.Bullet);
                            //Debug.Log($"bullet spwan {position.Value.y } {compositeScale.Value.c1.y}");
                            position.Value.y += (compositeScale.Value.c1.y - 0.5f) * 0.5f;//防止发射高度碰到地板,默认值是0.5，超过0.5的需要缩放
                        }

                        commandBuffer.SetComponent(entityInQueryIndex, bullet, position);
                        commandBuffer.SetComponent(entityInQueryIndex, bullet, rotation);
                        commandBuffer.SetComponent(entityInQueryIndex, bullet, velocity);
                    }
                    gun.Duration = 0;
                }
                gun.WasFiring = 1;
            }).ScheduleParallel();

        m_EntityCommandBufferSystem.AddJobHandleForProducer(Dependency);
    }
}
#endregion
