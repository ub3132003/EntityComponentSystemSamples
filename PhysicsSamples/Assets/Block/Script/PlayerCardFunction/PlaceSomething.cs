using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypertonic.GridPlacement;
using Unity.Entities;

public class PlaceSomething : MonoBehaviour
{
    [SerializeField] GameObject placePrefab;
    //boardcast on
    [SerializeField] GameObjectEventChannelSO placeObjEvent;
    [SerializeField] VoidEventChannelSO confirmPlaceObjEvent;

    private GameObject placeObject;

    public GameObject Create()
    {
        return Instantiate(placePrefab);
    }

    //TODo 失败时删除实例
    public void EnterPlaceMode()
    {
        placeObject = Create();
        GridManagerAccessor.GridManager.EnterPlacementMode(placeObject);
        placeObjEvent.RaiseEvent(placeObject);
    }

    public void excutePlace()
    {
        //Todo 直接由ecs 交互ui
        confirmPlaceObjEvent.RaiseEvent();
        placeObject.AddComponent<ConvertToEntity>();
    }
}
