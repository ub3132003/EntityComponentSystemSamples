using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;

/// <summary>
/// 阳光实体
/// </summary>
public struct SunCoinComponent : IComponentData
{
    public int Value;
}

[DisallowMultipleComponent]
public class SunCoinComponentAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public float scale;
    public int Val = 50;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        //dstManager.AddComponentData(entity, new Unity.Transforms.Scale { Value = scale });
        dstManager.AddComponentData(entity, new SunCoinComponent { Value = Val });
    }
}
