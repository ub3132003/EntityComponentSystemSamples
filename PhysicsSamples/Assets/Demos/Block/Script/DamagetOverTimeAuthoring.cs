using System.Collections;
using Unity.Entities;
using UnityEngine;


public struct DamagetOverTime : IComponentData, IDamage
{
    public int Value { get; set; }
    public COST_TYPES Type { get; set; }

    public BlobAssetReference<BuffBlobAsset> buffRef;
}


public class DamagetOverTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public RpgEffectSO effectSO;
    public int Value;
    public COST_TYPES Type;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var buffBlob = BuffBlobAsset.RegisterBlobAsset(effectSO);

        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref buffBlob);

        dstManager.AddComponentData(entity, new DamagetOverTime
        {
            Value = Value,
            Type = Type,
            buffRef = buffBlob
        });
    }
}
