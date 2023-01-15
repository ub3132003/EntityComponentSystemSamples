using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct Drone : IComponentData
{
    public float3 position;
    public float3 velocity;
    public float3 smoothPosition;
    public float3 smoothDirection;
    public int team;
    public float size;
    //public Entity enemyTarget;
    public Entity resourceTarget;
    /// <summary>
    /// 目的地
    /// </summary>
    public float3 resourceDestination;

    public bool dead;
    public float deathTimer;
    public bool isAttacking;
    public bool isHoldingResource;
    //public int resourceHolderTeam;
    public int index;
    /// <summary>
    /// 抓取资源时调用
    /// </summary>
    /// <param name="resourceTeam"></param>
    public void SetReource()
    {
        isHoldingResource = true;
    }

    /// <summary>
    /// 丢弃资源时调用
    /// </summary>
    public void ClearResource() { resourceTarget = Entity.Null;  isHoldingResource = false; }

    public void Init(float3 myPosition, int myTeam, float mySize)
    {
        position = myPosition;
        velocity = float3.zero;
        smoothPosition = position + math.right() * .01f;
        smoothDirection = float3.zero;
        velocity = float3.zero;
        team = myTeam;
        size = mySize;

        dead = false;
        deathTimer = 1f;
        isAttacking = false;
        isHoldingResource = false;
        index = -1;

        //enemyTarget = null;
        //resourceTarget = null;
    }
}
/// <summary>
/// 共享配置
/// </summary>
public struct DroneSettings : ISharedComponentData
{
    public float speedStretch;
    public float rotationStiffness;
    //[Range(0f, 1f)]
    public float aggression;
    public float flightJitter;
    public float teamAttraction;
    public float teamRepulsion;
    //[Range(0f, 1f)]
    public float damping;
    /// <summary>
    /// 前往目标的加速
    /// </summary>
    public float chaseForce;
    /// <summary>
    /// 携带物品时的加速
    /// </summary>
    public float carryForce;
    public float grabDistance;
    public float attackDistance;
    public float attackForce;
    public float hitDistance;
    public float maxSpawnSpeed;
}
