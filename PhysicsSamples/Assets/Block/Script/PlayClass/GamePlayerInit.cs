using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Systems;
using Unity.Transforms;
using Unity.Burst;
public class GamePlayerInit : MonoBehaviour, IConvertGameObjectToEntity, IDeclareReferencedPrefabs
{
    public Transform MainCharacterSpawnPoint;

    [Header("Prefabs")]
    public GameObject MainCharacterPrefab;


    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new GamePlayerInitialization
        {
            MainCharacterSpawnPoint = new RigidTransform(MainCharacterSpawnPoint.rotation, MainCharacterSpawnPoint.position),
            MainCharacterPrefabEntity = conversionSystem.GetPrimaryEntity(MainCharacterPrefab),

            //StartingCameraForward = MainCharacterSpawnPoint.forward,
        });
    }

    public void DeclareReferencedPrefabs(List<GameObject> referencedPrefabs)
    {
        referencedPrefabs.Add(MainCharacterPrefab);
    }
}


[System.Serializable]
public struct GamePlayerInitialization : IComponentData
{
    public RigidTransform MainCharacterSpawnPoint;
    public Entity MainCharacterPrefabEntity;
    //public float3 StartingCameraForward;
}
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial class GamePlayerInitializationSystem : SystemBase
{
    protected override void OnCreate()
    {
        base.OnCreate();
        Debug.Log("初始化角色");
    }

    protected override void OnUpdate()
    {
        if (HasSingleton<GamePlayerInitialization>())
        {
            GamePlayerInitialization sceneInitializer = GetSingleton<GamePlayerInitialization>();


            // Spawn main character
            Entity mainCharacterEntity = EntityManager.Instantiate(sceneInitializer.MainCharacterPrefabEntity);

            EntityManager.SetComponentData(mainCharacterEntity, new Translation { Value = sceneInitializer.MainCharacterSpawnPoint.pos });
            EntityManager.SetComponentData(mainCharacterEntity, new Rotation { Value = sceneInitializer.MainCharacterSpawnPoint.rot });

            //缓存实体
            PlayerEcsConnect.Instance.RegistPlayer(mainCharacterEntity);

            // Remove sceneInitializer component
            EntityManager.RemoveComponent<GamePlayerInitialization>(GetSingletonEntity<GamePlayerInitialization>());
        }
    }
}
