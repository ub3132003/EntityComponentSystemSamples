using Hypertonic.GridPlacement;
using Sirenix.Utilities;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class PlaceLauncher : MonoBehaviour
{
    [SerializeField] GameObject placePrefab;
    [SerializeField] ThingSO ballTtye;

    private GameObject placeObject;
    public GameObject CreateLauncher()
    {
        return Instantiate(placePrefab);
    }

    //TODo 失败时删除实例
    public void EnterPlaceMode()
    {
        placeObject = CreateLauncher();
        GridManagerAccessor.GridManager.EnterPlacementMode(placeObject);
    }

    public void excutePlace()
    {
        GridManagerAccessor.GridManager.ConfirmPlacement();
        //外观
        Instantiate(ballTtye.Prefab.transform.GetChild(0), transform);//外观

        //绑定实例和对象,在子对象中
        var placeEntity = placeObject.transform.Find("Entity").gameObject;
        placeEntity.AddComponent<EntitySender>().EntityReceivers = new GameObject[] { placeObject };

        //设置发射物属性
        var gunData = placeEntity.GetComponent<CharacterGunAuthoring>();
        gunData.Bullet = ballTtye.Prefab;
        placeEntity.AddComponent<ConvertToEntity>();
    }
}
