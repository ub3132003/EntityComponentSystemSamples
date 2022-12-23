using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class ViewChangeAbleAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public List<GameObject> ViewPrefabList;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        List<Entity> viewEntitys = new List<Entity>();
        for (int i = 0; i < ViewPrefabList.Count; i++)
        {
            var viewEntity = conversionSystem.GetPrimaryEntity(ViewPrefabList[i]);
            viewEntitys.Add(viewEntity);
        }

        var blob = BlobAssetHelp.BuildBlobAsset<PrefabBlobAsset, Entity[]>(viewEntitys.ToArray(), init, Unity.Collections.Allocator.Persistent);


        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref blob);

        dstManager.AddComponentData(entity, new ViewChangeAble { ViewPrefabBlob = blob });
        var child = dstManager.Instantiate(viewEntitys[0]);
        EntityHelp.SetParent(dstManager, entity, child);
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(ViewPrefabList);
    }

    private void init(ref PrefabBlobAsset blobData, Entity[] viewEntitys)
    {
        var build = BlobAssetHelp.BlobBuilder;
        var arr = build.Allocate(ref blobData.PrefabArray, viewEntitys.Length);
        for (int i = 0; i < viewEntitys.Length; i++)
        {
            arr[i] = viewEntitys[i];
        }
    }
}
