using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
/// <summary>
/// Blob 引用外观数组
/// </summary>
public struct PrefabBlobAsset
{
    public BlobArray<Entity> PrefabArray;
}

public struct ViewChangeAble : IComponentData
{
    public BlobAssetReference<PrefabBlobAsset> ViewPrefabBlob;
}
