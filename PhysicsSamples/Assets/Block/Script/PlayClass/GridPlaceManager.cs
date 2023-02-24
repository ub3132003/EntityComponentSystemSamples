using System.Collections;
using System.Collections.Generic;
using Unity.Burst.Intrinsics;
using Unity.Entities;
using UnityEngine;

using Unity.Physics.Extensions;
using Unity.Mathematics;
using Unity.Transforms;
/// <summary>
/// 建造操作，与玩家互动
/// </summary>
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

    bool isLeftHold;
    bool isRightHold;
    PlaceItemSetting _placeItemSetting = new PlaceItemSetting();
    PlaceItemSetting _previousItemSetting = new PlaceItemSetting();
    /// <summary>
    /// 去重集合
    /// </summary>
    private Dictionary<GameObject, Entity> _placePrefabDict = new Dictionary<GameObject, Entity>();
    private void OnEnable()
    {
        placeViewObjChangeEvent.OnEventRaised += OnPlaceViewObjectChange;
        placePerfabSetEvent.OnEventRaised += OnPlacePerfabSet;


        input.MouseLeftPress += OnPressMouseLeft;
        input.MouseRightPress += OnPressMouseRight;
        input.MouseLeftHoldEnter += OnHoldMouseLeft;
        input.MouseLeftHoldQuit += OnHoldQuitLeft;
        input.MouseRightHoldEnter += OnMouseHoldEnterRight;
        input.MouseRightHoldQuit += OnMouseHoldQuitRight;
    }

    private void OnDisable()
    {
        placeViewObjChangeEvent.OnEventRaised -= OnPlaceViewObjectChange;
        placePerfabSetEvent.OnEventRaised -= OnPlacePerfabSet;


        input.MouseLeftPress -= OnPressMouseLeft;
        input.MouseRightPress -= OnPressMouseRight;
        input.MouseLeftHoldEnter -= OnHoldMouseLeft;
        input.MouseLeftHoldQuit -= OnHoldQuitLeft;
        input.MouseRightHoldEnter -= OnMouseHoldEnterRight;
        input.MouseRightHoldQuit -= OnMouseHoldQuitRight;
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
        if (_currentPlaceViewObject == null) return;
        var diractionview = Instantiate(_perviewObject, _currentPlaceViewObject.transform);
        diractionview.transform.localPosition = Vector3.zero;
    }

    /// <summary>
    /// 按下鼠标左键后在判断的位置生成实体
    /// </summary>
    void OnPressMouseLeft()
    {
        //没有选择要放置的物体
        if (_readyPlacePerfab == null)
        {
            if (_currentEntity != Entity.Null)
            {
                if (_entityManager.HasComponent<EntityEventComponent>(_currentEntity))
                {
                    var entityEvent = _entityManager.GetSharedComponentManaged<EntityEventComponent>(_currentEntity);
                    entityEvent.entityEvent?.RaiseEvent(_currentEntity);
                }
            }
            return;
        }
        Entity readyPlaceEntityPrefab;
        readyPlaceEntityPrefab = GameEntityAssetManager.Instance.GetPrimaryEntity(_readyPlacePerfab);
        //避免重复转换实体
        //if (_placePrefabDict.TryGetValue(_readyPlacePerfab, out readyPlaceEntityPrefab))
        //{
        //}
        //else
        //{
        //    //对物理系统需要blob store？
        //    var _settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _conversionSystem.BlobAssetStore);
        //    readyPlaceEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(_readyPlacePerfab, _settings);
        //     _placePrefabDict.Add(_readyPlacePerfab, readyPlaceEntityPrefab);
        //}

        _placeItemSetting.PlaceEntityPerfab = readyPlaceEntityPrefab;

        CreateEntity(_placeItemSetting);
        //var entityInstance = _entityManager.Instantiate(readyPlaceEntityPrefab);
        //_entityManager.SetComponentData(entityInstance, new Translation { Value = _readyPlacePosition });
    }

    /// <summary>
    /// 连续建造
    /// </summary>
    void OnHoldMouseLeft()
    {
        isLeftHold = true;//Todo 切换放置物品时结束hold 状态
    }

    void OnHoldQuitLeft()
    {
        isLeftHold = false;
        _placeItemSetting = new PlaceItemSetting();
    }

    void OnMouseHoldEnterRight()
    {
        isRightHold = true;//Todo 切换放置物品时结束hold 状态
    }

    void OnMouseHoldQuitRight()
    {
        isRightHold = false;
    }

    /// <summary>
    /// 鼠标右键删除实体
    /// </summary>
    void OnPressMouseRight()
    {
        DeleteEntity();
    }

    MouseHover GetMouseHitInfo()
    {
        var mouseHoverEntity = _mouseSys.GetSingletonEntity<MouseHover>();
        var mouseHover = _entityManager.GetSharedComponentData<MouseHover>(mouseHoverEntity);

        return mouseHover;
    }

    ConvertToEntitySystem _conversionSystem;
    EntityManager _entityManager;
    MouseHoverSystem _mouseSys;
    Entity _currentEntity;
    EntityCommandBufferSystem _endEcbSys;
    private void Start()
    {
        _conversionSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ConvertToEntitySystem>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        _mouseSys = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<MouseHoverSystem>();
        _endEcbSys = World.DefaultGameObjectInjectionWorld.GetExistingSystem<EndSimulationEntityCommandBufferSystem>();
    }

    //[SerializeField] Vector3 debugValue;
    void Update()
    {
        //预览当前操作对象
        var mouseHover =  GetMouseHitInfo();
        _currentEntity = mouseHover.CurrentEntity;
        if (_currentEntity == Entity.Null) return;
        if (_currentPlaceViewObject == null) return;

        //要放到的位置方向
        var nextOffset = math.round(mouseHover.HitData.SurfaceNormal);
        //要放到的单元格位置
        var currentPosition = _entityManager.GetComponentData<LocalToWorld>(_currentEntity).Position;
        _readyPlacePosition = currentPosition + nextOffset;
        _currentPlaceViewObject.transform.position = _readyPlacePosition;
        _placeItemSetting.Position = new Translation { Value = _readyPlacePosition };

        //返回是float型的！这个由滚轮向前（正数）还是向后（负数）滚决定
        var scale = Input.GetAxis("Mouse ScrollWheel");
        if (scale != 0)
        {
            _currentPlaceViewObject.transform.Rotate(new Vector3(0, 90 * scale, 0));
            _placeItemSetting.Rotate = new Rotation { Value = _currentPlaceViewObject.transform.rotation };

            //GridManagerAccessor.GridManager.HandleGridObjectRotated();
        }
        //Debug.Log(scale);
        if (isLeftHold && isRightHold) return;
        //连续放置
        if (isLeftHold)//TODo 旋转 和抽象创建函数
        {
            //只能在更低层高度连续放置;不能建造更高的地方
            if (_previousItemSetting.Position.Value.y < _placeItemSetting.Position.Value.y) return;
            if (_previousItemSetting.Position.Value.Equals(_placeItemSetting.Position.Value)) return;
            CreateEntity(_placeItemSetting);
        }
        //连续删除
        if (isRightHold)
        {
            DeleteEntity();
        }
    }

    void CreateEntity(PlaceItemSetting placeItemSetting)
    {
        var entityInstance = _entityManager.Instantiate(placeItemSetting.PlaceEntityPerfab);
        _entityManager.SetComponentData(entityInstance, placeItemSetting.Position);
        _entityManager.SetComponentData(entityInstance, placeItemSetting.Rotate);
        _entityManager.AddComponentData(entityInstance, new PlayerCanDelEntity());
        _previousItemSetting = new PlaceItemSetting(placeItemSetting);
        Debug.Log(placeItemSetting);
    }

    void DeleteEntity()
    {
        if (_currentEntity == Entity.Null) return;
        if (!_entityManager.HasComponent<PlayerCanDelEntity>(_currentEntity)) return;
        _entityManager.AddComponentData(_currentEntity, new LifeTime { Value = 1 });
        _currentEntity = Entity.Null;
    }

    class PlaceItemSetting
    {
        public Entity PlaceEntityPerfab;
        public Rotation Rotate;
        public Translation Position;
        public PlaceItemSetting()
        {
            PlaceEntityPerfab = Entity.Null;
            Rotate.Value = quaternion.identity;
            Position.Value = float3.zero;
        }

        public PlaceItemSetting(PlaceItemSetting placeItemSetting)
        {
            PlaceEntityPerfab = placeItemSetting.PlaceEntityPerfab;
            Rotate = placeItemSetting.Rotate;
            Position = placeItemSetting.Position;
        }

        public override string ToString()
        {
            return $"{PlaceEntityPerfab},{Rotate.Value},{Position.Value}";
        }
    }
}
