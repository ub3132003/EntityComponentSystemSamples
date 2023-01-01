using Hypertonic.GridPlacement;
using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using UnityEngine;

using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;

public class GridPlaceManager : Singleton<GridPlaceManager>
{
    //外观
    private GameObject _currentPlaceViewObject;
    //要放的预制体
    private GameObject _readyPlacePerfab;
    // 将要生成物体的位置
    float3 _readyPlacePosition;
    //预览放置的外观 指示方向
    [SerializeField] GameObject _perviewObject;
    //listing in
    [SerializeField] GameObjectEventChannelSO placeViewObjChangeEvent;
    [SerializeField] GameObjectEventChannelSO placePerfabSetEvent;
    [SerializeField] VoidEventChannelSO confirmPlaceObjEvent;

    [SerializeField] InputReader input;
    /// <summary>
    /// 去重集合
    /// </summary>
    private Dictionary<GameObject, Entity> _placePrefabDict = new Dictionary<GameObject, Entity>();
    private void OnEnable()
    {
        placeViewObjChangeEvent.OnEventRaised += OnPlaceViewObjectChange;
        placePerfabSetEvent.OnEventRaised += OnPlacePerfabSet;
        confirmPlaceObjEvent.OnEventRaised += HandleConfirmButtonPressed;

        input.MouseLeftPress += OnPressMouseLeft;
    }

    private void OnDisable()
    {
        placeViewObjChangeEvent.OnEventRaised -= OnPlaceViewObjectChange;
        placePerfabSetEvent.OnEventRaised -= OnPlacePerfabSet;
        confirmPlaceObjEvent.OnEventRaised -= HandleConfirmButtonPressed;

        input.MouseLeftPress -= OnPressMouseLeft;
    }

    /// <summary>
    /// 设置要放的预制体
    /// </summary>
    /// <param name="obj"></param>
    void OnPlacePerfabSet(GameObject obj)
    {
        _readyPlacePerfab = obj;
    }

    void OnPlaceViewObjectChange(GameObject obj)
    {
        _currentPlaceViewObject = obj;
        _perviewObject.transform.SetParent(_currentPlaceViewObject.transform);
    }

    /// <summary>
    /// 按下鼠标左键后在判断的位置生成实体
    /// </summary>
    void OnPressMouseLeft()
    {
        Entity readyPlaceEntityPrefab;
        //避免重复转换实体
        if (_placePrefabDict.TryGetValue(_readyPlacePerfab, out readyPlaceEntityPrefab))
        {
        }
        else
        {
            readyPlaceEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_readyPlacePerfab, _settings);
            _placePrefabDict.Add(_readyPlacePerfab, readyPlaceEntityPrefab);
        }

        var entityInstance = _entityManager.Instantiate(readyPlaceEntityPrefab);
        _entityManager.SetComponentData(entityInstance, new Translation { Value = _readyPlacePosition });
    }

    MouseHover GetMouseHitInfo()
    {
        var mouseHoverEntity = _mouseSys.GetSingletonEntity<MouseHover>();
        var mouseHover = _entityManager.GetSharedComponentData<MouseHover>(mouseHoverEntity);
        return mouseHover;
    }

    GameObjectConversionSettings _settings;
    EntityManager _entityManager;
    MouseHoverSystem _mouseSys;
    private void Start()
    {
        var conversionSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ConvertToEntitySystem>();
        _settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, conversionSystem.BlobAssetStore);//对物理系统需要blob store？
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _mouseSys = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MouseHoverSystem>();
    }

    //[SerializeField] Vector3 debugValue;
    void Update()
    {
        if (_currentPlaceViewObject == null)
        {
            return;
        }
        //预览当前操作对象
        var mouseHover =  GetMouseHitInfo();
        //要放到的位置方向
        var nextOffset = math.round(mouseHover.HitData.SurfaceNormal);
        var currentEntity = mouseHover.CurrentEntity;
        if (currentEntity != Entity.Null)
        {
            //要放到的单元格位置
            var nextPosition = _entityManager.GetComponentData<LocalToWorld>(currentEntity).Position + nextOffset;
            _readyPlacePosition = nextPosition;
            _currentPlaceViewObject.transform.position = nextPosition;

            //返回是float型的！这个由滚轮向前（正数）还是向后（负数）滚决定
            var scale = Input.GetAxis("Mouse ScrollWheel");
            if (scale != 0)
            {
                _currentPlaceViewObject.transform.Rotate(new Vector3(0, 90 * scale, 0));
                //GridManagerAccessor.GridManager.HandleGridObjectRotated();
            }
            //Debug.Log(scale);
        }
    }

    private void HandleConfirmButtonPressed()
    {
        bool placed = GridManagerAccessor.GridManager.ConfirmPlacement();

        if (placed)
        {
            _currentPlaceViewObject = null;
        }
    }

    private void HandleCancelPlacementPressed()
    {
        GridManagerAccessor.GridManager.CancelPlacement();
        _currentPlaceViewObject = null;
    }

    private void HandleDeleteObjectPressed()
    {
        GridManagerAccessor.GridManager.DeleteObject(_currentPlaceViewObject);
        _currentPlaceViewObject = null;
    }

    private void HandleRotateLeftPressed()
    {
        //_selectedGridObject.transform.Rotate(new Vector3(0, -90, 0));

        GridManagerAccessor.GridManager.HandleGridObjectRotated();
    }

    private void HandleRotateRightPressed()
    {
        //_selectedGridObject.transform.Rotate(new Vector3(0, 90, 0));

        GridManagerAccessor.GridManager.HandleGridObjectRotated();
    }
}
