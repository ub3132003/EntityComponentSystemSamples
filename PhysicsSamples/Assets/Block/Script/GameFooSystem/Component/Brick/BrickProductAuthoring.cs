using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class BrickProductAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    [SerializeField] List<GameObject> InBulletList;
    [SerializeField] List<GameObject> OutBulletList;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var recipe = new BrickProductRecipe
        {
            InA = InBulletList[0].GetInstanceID(),
            InB = InBulletList[1].GetInstanceID(),
            OutA = conversionSystem.GetPrimaryEntity(OutBulletList[0])
        };
        var recipeData = BlobAssetHelp.CreateReference(recipe, Unity.Collections.Allocator.Persistent);
        dstManager.AddComponentData(entity, new BrickProduct
        {
            RecipeBlob = recipeData
        });
        dstManager.AddComponentData(entity, new BrickCacheBullet());
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.AddRange(OutBulletList);
    }
}
