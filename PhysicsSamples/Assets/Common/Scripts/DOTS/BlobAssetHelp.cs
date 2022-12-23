using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public static class BlobAssetHelp
{
    private static BlobBuilder BLOB_BUILDER;

    // We expose this to the clients to allow them to create BlobArray using BlobBuilderArray
    public static BlobBuilder BlobBuilder => BLOB_BUILDER;

    // We allow the client to pass an action containing their blob creation logic
    public delegate void ActionRef<TBlobAssetType, in TDataType>(ref TBlobAssetType blobAsset, TDataType data);

    public static BlobAssetReference<TBlobAssetType> BuildBlobAsset<TBlobAssetType, TDataType>
        (TDataType data, ActionRef<TBlobAssetType, TDataType> action , Allocator allocator) where TBlobAssetType : struct
    {
        BLOB_BUILDER = new BlobBuilder(Allocator.Temp);

        // Take note of the "ref" keywords. Unity will throw an error without them, since we're working with structs.
        ref TBlobAssetType blobAsset = ref BLOB_BUILDER.ConstructRoot<TBlobAssetType>();

        // Invoke the client's blob asset creation logic
        action.Invoke(ref blobAsset, data);

        // Store the created reference to the memory location of the blob asset, before disposing the builder
        BlobAssetReference<TBlobAssetType> blobAssetReference = BLOB_BUILDER.CreateBlobAssetReference<TBlobAssetType>(allocator);

        // We're not in a Using block, so we manually dispose the builder
        BLOB_BUILDER.Dispose();

        // Return the created reference
        return blobAssetReference;
    }

    public static BlobAssetReference<T> CreateReference<T>(T value, Allocator allocator) where T : struct
    {
        BlobBuilder builder = new BlobBuilder(Allocator.TempJob);
        ref T data = ref builder.ConstructRoot<T>();
        data = value;
        BlobAssetReference<T> reference = builder.CreateBlobAssetReference<T>(allocator);
        builder.Dispose();

        return reference;
    }
}
