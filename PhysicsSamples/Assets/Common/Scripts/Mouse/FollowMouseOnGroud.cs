using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
[Serializable]
[GenerateAuthoringComponent]
public struct FollowMouseOnGroud : IComponentData
{
    public float3 offset;
    public float StopDistance;
    public float3 MaxSpeed;
    // 移动比例
    public float MoveSpeed;

    //y加速强度
    public float3 HitForce;
}
/// <summary>
/// 按照鼠标移动增量移动物体
/// </summary>
public partial class MouseMoveInput : SystemBase
{
    protected override void OnUpdate()
    {
        //跟随鼠标 没有y轴

        var dx = Input.GetAxis("Mouse X");
        var dy = Input.GetAxis("Mouse Y");
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
