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
            InA = conversionSystem.GetPrimaryEntity(InBulletList[0]),
            InB = conversionSystem.GetPrimaryEntity(InBulletList[1]),
            OutA = conversionSystem.GetPrimaryEntity(OutBulletList[0])
        };
        var recipeData = BlobAssetHelp.CreateReference(recipe, Unity.Collections.Allocator.Persistent);
        dstManager.AddComponentData(entity, new BrickProduct
        {
            RecipeBlob = recipeData
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        foreach (var item in InBulletList)
        {
            referencedPrefabs.Add(item);
        }
        foreach (var item in OutBulletList)
        {
            referencedPrefabs.Add(item);
        }
    }
}
