using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using UnityEngine;
using Random = Unity.Mathematics.Random;
using System.Collections.Generic;

struct TestMoveToTargetPoint : IComponentData
{
    public Entity Prefab;
}
public class TestMoveToTargetPointAuthing : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public GameObject Prefab;
    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(Prefab);
    }

    //这个实体是spwaner
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var spawnerData = new TestMoveToTargetPoint
        {
            // The referenced prefab will be converted due to DeclareReferencedPrefabs.
            // So here we simply map the game object to an entity reference to that prefab.
            Prefab = conversionSystem.GetPrimaryEntity(Prefab),
        };
        //MoveData moveData = new MoveData
        //{
        //    MoveSpeed = 1,
        //    Direction = math.up()
        //};
        //dstManager.AddComponentData(entity, moveData);
        dstManager.AddComponentData(entity, spawnerData);
    }
}
