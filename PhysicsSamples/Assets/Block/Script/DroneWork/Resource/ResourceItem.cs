using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

[Serializable]
public struct ResourceItem : IComponentData
{
    public float3   position;
    public bool     stacked;
    public int      stackIndex;
    public int      gridX;
    public int      gridY;
    public Entity   holder;//dots 1.0 you aspet 组件可以引用组件而不是实体
    public bool     hasHolder => holder != Entity.Null;
    public int      holderTeam;//持有者的分组
    public float3   velocity;
    public bool     dead;

    public void SetHolder(int holderTeam)
    {
        this.holderTeam = holderTeam;
    }

    public void ClearHolder() { holder = Entity.Null; holderTeam = -1; }

    public ResourceItem(float3 myPosition)
    {
        position = myPosition;
        stacked = default;
        stackIndex = default;
        gridX = default;
        gridY = default;
        holder = default;
        velocity = default;
        dead = default;
        holderTeam = -1;
    }
}

public struct ResourceItemSetting : ISharedComponentData
{
    public float resourceSize;
    public float snapStiffness;
    /// <summary>
    /// 重量？
    /// </summary>
    public float carryStiffness;
    public float spawnRate;
    public int beesPerResource;
}
