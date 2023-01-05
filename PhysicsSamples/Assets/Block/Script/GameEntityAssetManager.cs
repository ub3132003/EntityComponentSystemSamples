using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using Unity.Entities;
using Unity.Physics.Extensions;
using UnityEditor;
using UnityEngine;

public class GameEntityAssetManager : Singleton<GameEntityAssetManager>
{
    //因为conver 重复转换成不唯一实体，所以这在里统一存储
    public Dictionary<GameObject, Entity> PerfabEntityDict = new Dictionary<GameObject, Entity>();

    ConvertToEntitySystem _conversionSystem;
    EntityManager _entityManager;

    Entity _currentEntity;
    /// <summary>
    /// 子弹集合
    /// </summary>
    [SerializeField] List<ThingSO> _ballAssets;

    public List<ThingSO> BallAssets { get => _ballAssets; }

    protected override void Awake()
    {
        base.Awake();
        _conversionSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<ConvertToEntitySystem>();
        _entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    /// <summary>
    /// 保证实体转后结果唯一
    /// </summary>
    /// <param name="obj"></param>
    /// <returns></returns>
    public Entity GetPrimaryEntity(GameObject obj)
    {
        var type = PrefabUtility.GetPrefabAssetType(obj);
        var status = PrefabUtility.GetPrefabInstanceStatus(obj);
        // 是否为预制体实例判断
        Assert.IsTrue(type == PrefabAssetType.NotAPrefab || status == PrefabInstanceStatus.NotAPrefab, "obj must be an asset prefab");

        Entity readyPlaceEntityPrefab;
        //避免重复转换实体
        if (PerfabEntityDict.TryGetValue(obj, out readyPlaceEntityPrefab))
        {
        }
        else
        {
            //对物理系统需要blob store？
            var _settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, _conversionSystem.BlobAssetStore);
            readyPlaceEntityPrefab = GameObjectConversionUtility.ConvertGameObjectHierarchy(obj, _settings);
            PerfabEntityDict.Add(obj, readyPlaceEntityPrefab);
        }
        return readyPlaceEntityPrefab;
    }
}
