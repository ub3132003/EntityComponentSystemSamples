using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

[DisallowMultipleComponent]
public class BlockIdAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int BlockId;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BlockID { blockID = BlockId });
    }
}
