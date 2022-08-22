using System.Collections;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

public struct BuffBlobAsset
{
    public int StackLimit;

    public int Pulses;
    public float Duration;
    public bool Endless;

    public static BlobAssetReference<BuffBlobAsset> RegisterBlobAsset(RpgEffectSO data)
    {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref BuffBlobAsset asset = ref blobBuilder.ConstructRoot<BuffBlobAsset>();

            asset.StackLimit = data.stackLimit;
            asset.Pulses = data.pulses;
            asset.Duration = data.duration;
            asset.Endless = data.endless;

            return blobBuilder.CreateBlobAssetReference<BuffBlobAsset>(Allocator.Persistent);
        }
    }
}

public struct BlobAnimationClip : IComponentData
{
    public BlobAssetReference<BuffBlobAsset> animBlobRef;
    public float timer;
    public int frame;
    public float3 localPosition;
}

[DisallowMultipleComponent]
public class BuffBolbAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] RpgEffectSO effectSO;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var buffBlob = BuffBlobAsset.RegisterBlobAsset(effectSO);

        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref buffBlob);

        //dstManager.AddComponentData(entity, new BuffEffectComponent()
        //{
        //    buffRef = buffBlob,
        //});
    }
}
