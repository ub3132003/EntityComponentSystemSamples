using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct AbillitySpawnComponent : IBufferElementData
{
    public Entity Abillity;
    public Entity Target;

    public float Delay;


    public double StartTime;
    public enum TARGET_TYPES
    {
        SELF,
        CONE,
        AOE,
        LINEAR,
        PROJECTILE,
        SQUARE,
        GROUND,
        GROUND_LEAP,
        TARGET_PROJECTILE,
        TARGET_INSTANT
    }
    public TARGET_TYPES TargetType;
    public enum TARGET_TYPE
    {
        Target,
        Caster
    }
}
class AbillityMulSpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public List<GameObject> AbPerfabList;

    public float Delay;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var abBuffer = dstManager.AddBuffer<AbillitySpawnComponent>(entity);
        foreach (var AbPerfab in AbPerfabList)
        {
            abBuffer.Add(new AbillitySpawnComponent
            {
                Abillity = conversionSystem.GetPrimaryEntity(AbPerfab),
                Delay = Delay
            });
        }
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        foreach (var AbPerfab in AbPerfabList)
        {
            referencedPrefabs.Add(AbPerfab);
        }
    }
}
