using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Transforms;

public class UiManager : MonoBehaviour
{
    EntityManager _entityManager => World.DefaultGameObjectInjectionWorld.EntityManager;
    //lisenting in
    [SerializeField] EntityChannelSO _cilckEntityEvent;

    Entity _currentEntity;

    public UISelectLaunchBall SelectLaunchBall;
    private void Start()
    {
        SelectLaunchBall.Close();
        SelectLaunchBall.InitSelectOption(GameEntityAssetManager.Instance.BallAssets);
        SelectLaunchBall.dropdown.onValueChanged.AddListener(SetLaunchBall);
    }

    private void OnEnable()
    {
        _cilckEntityEvent.OnEventRaised += OnCilckEntity;
    }

    private void OnDisable()
    {
        _cilckEntityEvent.OnEventRaised -= OnCilckEntity;
    }

    void OnCilckEntity(Entity e)
    {
        Debug.Log(e);
        if (e != _currentEntity)
        {
            _currentEntity = e;
            OpenWindowAtPosition(e);
        }
    }

    /// <summary>
    /// 根据点击物体打开对应窗口
    /// </summary>
    /// <param name="e"></param>
    void OpenWindow(Entity e)
    {
        if (_entityManager.HasComponent<CharacterGun>(e))
        {
            //展示当前实体所包含的数据
            GetLaunchBall(e);
            SelectLaunchBall.Open();
        }
    }

    void OpenWindowAtPosition(Entity e)
    {
        var worldPos = _entityManager.GetComponentData<LocalToWorld>(e).Position;
        var uiPos = RectTransformExtensions.RectTransformExtension.ConverToWorldPoint(transform as RectTransform, worldPos, Vector2.right);
        (SelectLaunchBall.transform as RectTransform).anchoredPosition = uiPos;
        OpenWindow(e);
    }

    //逻辑控制部分

    /// <summary>
    /// 设置发射球界面回调
    /// </summary>
    /// <param name="index">球在数组中的序号 </param>
    void SetLaunchBall(int index)
    {
        var ball = GameEntityAssetManager.Instance.BallAssets[index];

        var gun = _entityManager.GetComponentData<CharacterGun>(_currentEntity);
        gun.Bullet = GameEntityAssetManager.Instance.GetPrimaryEntity(ball.Prefab);
        gun.ID = ball.Prefab.GetInstanceID();
        _entityManager.SetComponentData(_currentEntity, gun);
    }

    //展示当前的球
    void GetLaunchBall(Entity entity)
    {
        var gun = _entityManager.GetComponentData<CharacterGun>(entity);
        var ballIndex = GameEntityAssetManager.Instance.BallAssets.FindIndex((x) => x.Prefab.GetInstanceID() == gun.ID);
        SelectLaunchBall.dropdown.SetValueWithoutNotify(ballIndex);
    }
}
