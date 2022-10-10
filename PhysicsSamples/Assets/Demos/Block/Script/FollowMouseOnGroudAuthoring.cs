using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
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
}

public class FollowMouseOnGroudAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float3 MaxSpeed;

    public float MoveSpeed;

    public float3 HitForce;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new FollowMouseOnGroud
        {
            MaxSpeed = MaxSpeed,
            MoveSpeed = MoveSpeed,
            HitForce = HitForce,
        });
        //dstManager.AddComponentData(entity, new NonUniformScale
        //{
        //    Value = new float3(2, 0, 0)
        //});

        PlayerEcsConnect.Instance.RegistPlayer(entity);
    }
}

/// <summary>
/// 按照鼠标移动增量移动物体
/// </summary>
public partial class MouseMoveInput : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
    }

    protected override void OnUpdate()
    {
        //跟随鼠标 没有y轴

        //var dx = Input.GetAxis("Mouse X");
        //var dy = Input.GetAxis("Mouse Y");
        var input = GetSingleton<CharacterGunInput>();
        var dx = input.Looking.x;
        var dy = input.Looking.y;
        var deltaTime = Time.DeltaTime;
        //Camera.main.ScreenToWorldPoint(new Vector3(Input.mousePosition.x, Input.mousePosition.y, 1));

        Entities.ForEach((ref PhysicsVelocity pv, ref Translation t, in FollowMouseOnGroud followMouse) =>
        {
            //t.Value = new float3(hit.Position.x, 0, hit.Position.z) + followMouse.offset;

            //if (math.distance(hit.Position + followMouse.offset, t.Value) < followMouse.StopDistance)
            //{
            //    pv.Linear = 0;
            //}
            //else
            //{
            //    pv.Linear = math.normalizesafe(hit.Position + followMouse.offset - t.Value) * followMouse.MaxSpeed;
            //}


            var xspeed = math.clamp((dx * deltaTime) * followMouse.MoveSpeed, -followMouse.MaxSpeed.x, followMouse.MaxSpeed.x);
            var yspeed = math.clamp((dy * deltaTime) * followMouse.MoveSpeed, -followMouse.MaxSpeed.z, followMouse.MaxSpeed.z);

            pv.Linear = new float3(xspeed, 0, yspeed);
            //t.Value += new float3(xspeed, 0, yspeed);
        }).Schedule();
    }
}
