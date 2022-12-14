using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypertonic.GridPlacement;
public class PlaceSomething : MonoBehaviour
{
    [SerializeField] GameObject placePrefab;

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
    }

    public void excutePlace()
    {
        //Todo 直接由ecs 交互ui 
    }
}
