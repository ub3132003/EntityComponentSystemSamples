using Unity.Entities;
using Unity.Mathematics;
using UnityEditor.Rendering;
using UnityEngine;
/// <summary>
/// 暂时不用.
/// </summary>
[DisallowMultipleComponent]
public class DroneAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var drone = new Drone();
        dstManager.AddSharedComponentData(entity, new DroneSettings());
        dstManager.AddComponentData(entity, drone);
    }
}
