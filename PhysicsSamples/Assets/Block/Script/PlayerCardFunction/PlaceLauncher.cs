//using Sirenix.Utilities;
//using System.Collections;
//using System.Collections.Generic;
//using Unity.Entities;
//using UnityEngine;

//public class PlaceLauncher : MonoBehaviour
//{
//    [SerializeField] GameObject placePrefab;
//    [SerializeField] GameObject placeEntity;
//    [SerializeField] ThingSO ballTtye;

//    private GameObject placeObject;
//    public GameObject CreateLauncher()
//    {
//        return Instantiate(placePrefab);
//    }

//    //TODo 失败时删除实例
//    public void EnterPlaceMode()
//    {
//        placeObject = CreateLauncher();
//        GridManagerAccessor.GridManager.EnterPlacementMode(placeObject);
//    }

//    public void excutePlace()
//    {
//        GridManagerAccessor.GridManager.ConfirmPlacement();
//        //外观 添加
//        Instantiate(ballTtye.Prefab.transform.GetChild(0).gameObject, placeObject.transform);

//        //绑定实例和对象,在子对象中
//        placeObject.AddComponent<EntitySender>().EntityReceivers = new GameObject[] { placeObject };

//        var entityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(placeEntity, settings);
//        //设置发射物属性
//        var entityInstance = entityManager.Instantiate(entityPrefab);
//        var gunData = entityManager.GetComponentData<CharacterGun>(entityInstance);
//        gunData.Bullet = GameObjectConversionUtility.ConvertGameObjectHierarchy(ballTtye.Prefab, settings);
//        entityManager.SetComponentData(entityInstance, gunData);
//    }

//    GameObjectConversionSettings settings;

//    EntityManager entityManager;
//    BlobAssetStore assetStore;
//    private void Start()
//    {
//        assetStore = new BlobAssetStore();
//        settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);//TODO bug  缺少blobasset store

//        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
//    }

//    private void OnDestroy()
//    {
//        assetStore.Dispose();
//    }
//}
