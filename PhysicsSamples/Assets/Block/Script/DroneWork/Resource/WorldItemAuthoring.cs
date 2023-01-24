using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
using Unity.Rendering;
using Unity.Collections;

public struct WorldItem : IComponentData
{
    public FixedString128Bytes itemGuid;
}

[DisallowMultipleComponent]
public class WorldItemAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    //物品主键，用于索引世界物品，和背包物品
    public ItemSO ResouceItem;
    public Mesh ItemMesh;
    public Material ItemMatrial;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var renderMesh = dstManager.GetSharedComponentData<RenderMesh>(entity);
        renderMesh.mesh = ItemMesh;
        renderMesh.material = ItemMatrial;
        dstManager.SetSharedComponentData(entity, renderMesh);
        dstManager.AddComponentData(entity, new WorldItem
        {
            itemGuid = new FixedString128Bytes(ResouceItem.Guid),
        });
    }
}
