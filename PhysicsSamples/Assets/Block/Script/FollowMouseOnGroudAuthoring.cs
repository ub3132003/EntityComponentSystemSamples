using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using Unity.Physics.GraphicsIntegration;
using Unity.Physics.Systems;
/// <summary>
/// 角色基础信息 属性
/// </summary>
[Serializable]

public struct FollowMouseOnGroud : IComponentData
{
    public float3 MaxSpeed;
    // 移动比例
    public float MoveSpeed;

    //y加速强度
    public float3 HitForce;
    //跟随位置偏移，
    public float3 Offset;
}

public class FollowMouseOnGroudAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 MaxSpeed;

    public float MoveSpeed;

    public float3 HitForce;

    public float3 Offset;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FollowMouseOnGroud
        {
            MaxSpeed = MaxSpeed,
            MoveSpeed = MoveSpeed,
            HitForce = HitForce,
            Offset = Offset
        });
        //dstManager.AddComponentData(entity, new NonUniformScale
        //{
        //    Value = new float3(2, 0, 0)
        //});
    }
}

/// <summary>
/// 按照鼠标移动增量移动物体
/// </summary>
public partial class MouseMoveInput : SystemBase
{
    private Unity.Physics.RaycastHit hit;

    public Unity.Physics.RaycastHit MouseHit { get => hit; }

    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        //跟随鼠标 没有y轴

        //var dx = Input.GetAxis("Mouse X");
        //var dy = Input.GetAxis("Mouse Y");
        //var input = GetSingleton<CharacterGunInput>();
        //var dx = input.Looking.x;
        //var dy = input.Looking.y;
        var deltaTime = Time.DeltaTime;
        //Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));
        var physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Vector2 mousePosition = Input.mousePosition;
        UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(mousePosition);
        var rayInput = new RaycastInput
        {
            Start = unityRay.origin,
            End = unityRay.origin + unityRay.direction * 100f,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 11,
                GroupIndex = 0
            }
        };
        bool haveHit = collisionWorld.CastRay(rayInput, out hit);
        var mousehit = hit;
        if (haveHit)
        {
        }

        Entities.ForEach((ref PhysicsVelocity pv, ref Translation t, in FollowMouseOnGroud followMouse) =>
        {
            //dx = hit.Position.x - t.Value.x;
            //dy = hit.Position.y - t.Value.y;
            t.Value = mousehit.Position + followMouse.Offset;
            //var xspeed = math.clamp((dx * deltaTime) * followMouse.MoveSpeed, -followMouse.MaxSpeed.x, followMouse.MaxSpeed.x);
            //var yspeed = math.clamp((dy * deltaTime) * followMouse.MoveSpeed, -followMouse.MaxSpeed.z, followMouse.MaxSpeed.z);

            //pv.Linear = new float3(xspeed, 0, yspeed);
            //t.Value += new float3(xspeed, 0, yspeed);
        }).Schedule();
    }
}
