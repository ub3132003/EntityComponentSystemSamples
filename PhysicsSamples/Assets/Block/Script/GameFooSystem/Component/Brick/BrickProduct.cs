using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;

/// <summary>
/// 消耗 ab 物体，得到out a 物体 ,配方
/// </summary>
public struct BrickProductRecipe
{
    //todo blob 数组
    public int InA;
    public int InB;
    //Entity InC;
    public Entity OutA;
    //Entity OutB;
}

/// <summary>
/// 配方引用,可以合成物体
/// </summary>
public struct BrickProduct : IComponentData
{
    public BlobAssetReference<BrickProductRecipe> RecipeBlob;
    //记录存储数量
    public int InACount;
    public int InBCount;
}
