using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hypertonic.GridPlacement;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine.UI;
using Unity.Transforms;

public class PlaceSomething : MonoBehaviour
{
    [SerializeField] GameObject placeViewPerfab;
    [SerializeField] GameObject placePrefab;
    //boardcast on
    [SerializeField] GameObjectEventChannelSO placeObjEvent;
    [SerializeField] GameObjectEventChannelSO placeViewChangeEvent;
    [SerializeField] VoidEventChannelSO confirmPlaceObjEvent;

    private GameObject placeViewInstance;
    //代表的物体数组序号
    public int index;

    public GameObject Create()
    {
        return Instantiate(placePrefab);
    }

    //TODo 失败时删除实例
    public void EnterPlaceMode()
    {
        placeViewInstance = Instantiate(placeViewPerfab);
        //GridManagerAccessor.GridManager.EnterPlacementMode(placeObject);
        placeViewChangeEvent.RaiseEvent(placeViewInstance);
        placeObjEvent.RaiseEvent(placePrefab);
    }

    //public void excutePlace()
    //{
    //    //Todo 直接由ecs 交互ui
    //    confirmPlaceObjEvent.RaiseEvent();
    //}

    public void OnToggleValueChange(bool opt)
    {
        if (opt)
        {
            EnterPlaceMode();
        }
        else
        {
            // 取消之前的状态。
            Destroy(placeViewInstance);
        }
    }

    private void Start()
    {
    }
}
