using System;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;


public struct ColliderHitPsTag : IComponentData
{
    public int PsId;
}


public class ColliderHitPsTagAuthoring : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    private void OnValidate()
    {
        Assert.IsNotNull(ParticleSystemRef.GetComponent<ParticleSystem>());
    }

    /// <summary>
    /// 场景中的引用
    /// </summary>
    public GameObject ParticleSystemRef;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new ColliderHitPsTag()
        {
            PsId = ParticleSystemRef.GetComponent<ParticleSystem>().GetInstanceID(),
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(ParticleSystemRef);
}
//public class ColliderHitPsTagAuthoring : GameObjectConversionSystem
//{
//    /// <summary>
//    /// 场景中的引用
//    /// </summary>
//    public GameObject ParticleSystemRef;
//    protected override void OnUpdate()
//    {
//        dstManager.AddComponentData(entity, new ColliderHitPsTag()
//        {
//            PsId = ParticleSystemRef.GetInstanceID(),
//        });
//    }


//    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs) => referencedPrefabs.Add(ParticleSystemRef);
//}
