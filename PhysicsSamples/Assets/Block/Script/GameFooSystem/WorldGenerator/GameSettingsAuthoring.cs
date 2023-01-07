using System;
using Unity.Entities;
using UnityEngine;

class GameSettingsAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int chunkSize;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new GameSettings
        {
            chunkSize = chunkSize
        });
    }
}
