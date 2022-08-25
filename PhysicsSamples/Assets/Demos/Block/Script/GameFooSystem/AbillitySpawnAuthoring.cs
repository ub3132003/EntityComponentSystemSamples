using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public struct AbillitySpawnComponent : IComponentData
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
    public enum TARGET_TYPE
    {
        Target,
        Caster
    }
}
class AbillitySpawnAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject AbPerfab;

    public float Delay;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new AbillitySpawnComponent
        {
            Abillity = conversionSystem.GetPrimaryEntity(AbPerfab),
            Delay = Delay
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(AbPerfab);
    }
}
