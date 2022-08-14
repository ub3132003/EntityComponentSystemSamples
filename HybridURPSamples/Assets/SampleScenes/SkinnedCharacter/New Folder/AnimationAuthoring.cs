using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
 
using Unity.Jobs;
using Unity.Transforms;
using Unity.Burst;

using Unity.Collections;

public struct AnimationBlobAsset
{
    public float frameDelta;
    public int frameCount;
    public BlobArray<float3> positions;
    public BlobArray<float3> eulers;
    public BlobArray<float3> scales;
 
    public static BlobAssetReference<AnimationBlobAsset> RegisterBlobAsset(AnimationDataSO data)
    {
        //AnimationData data = animationData; // Asset◊ ‘¥º”‘ÿ

        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref AnimationBlobAsset asset = ref blobBuilder.ConstructRoot<AnimationBlobAsset>();
            BlobBuilderArray<float3> positions = blobBuilder.Allocate(ref asset.positions, data.frameCount);
            BlobBuilderArray<float3> scales = blobBuilder.Allocate(ref asset.scales, data.frameCount);
            asset.frameDelta = data.frameDelta;
            asset.frameCount = data.frameCount;

            for (int i = 0; i < data.frameCount; ++i)
            {
                positions[i] = new float3(data.positions[i]);
                scales[i] = new float3(data.scales[i]);
            }

            return blobBuilder.CreateBlobAssetReference<AnimationBlobAsset>(Allocator.Persistent);
        }
    }

}

public struct BlobAnimationClip : IComponentData
{
    public BlobAssetReference<AnimationBlobAsset> animBlobRef;
    public float timer;
    public int frame;
    public float3 localPosition; 
}

[DisallowMultipleComponent]
public class AnimationAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{

    [SerializeField]  AnimationDataSO animationData;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var animationBlob = AnimationBlobAsset.RegisterBlobAsset(animationData);

        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref animationBlob);

        dstManager.AddComponentData(entity, new BlobAnimationClip()
        {
            animBlobRef = animationBlob,
            timer = 0f,
            frame = 0,
        });

    }

}
 