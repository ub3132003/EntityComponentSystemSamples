using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;
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

    /// <summary>
    /// remove at 0 child ， appent index at viewData to parent
    /// </summary>
    /// <param name="ecb"></param>
    /// <param name="viewData"></param>
    /// <param name="viewIndex"></param>
    /// <param name="parent"></param>
    /// <param name="childs"></param>
    public static void ReplaceChild0(EntityCommandBuffer ecb, ViewChangeAble viewData, int viewIndex, Entity parent, DynamicBuffer<Child> childs)
    {
        var viewPrefab = viewData.ViewPrefabBlob.Value.PrefabArray[viewIndex];

        var newView = ecb.Instantiate(viewPrefab);
        EntityHelp.SetParent(ecb, parent, newView, float3.zero, quaternion.identity);
        ecb.AppendToBuffer(parent, new Child { Value = newView });
        ecb.DestroyEntity(childs[0].Value);
    }
}
