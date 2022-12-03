using RectTransformExtensions;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.UI;

using Unity.Entities;

using Unity.Physics;

/// <summary>
/// 点击塔时显示的操作界面
/// </summary>
public class TowerFunctionUI : MonoBehaviour
{
    [SerializeField] GameObjectEventChannelSO cilckEvent;
    [SerializeField] Button launchButton;
    [SerializeField] LaunchIndicatorLine launchIndicatorLine;
    //当前操作的对象
    private LauncherTower targetObj;
    private RectTransform rectTransform => gameObject.transform as RectTransform;
    private bool isLeftButtonPress;
    private int stateMode;
    private void OnDestroy()
    {
        cilckEvent.OnEventRaised -= Open;
    }

    void Start()
    {
        cilckEvent.OnEventRaised += Open;
        launchButton.onClick.AddListener(SetTowerToLaunch);
        gameObject.SetActive(false);
    }

    public void SetTowerToLaunch()
    {
        stateMode = 1;
        launchIndicatorLine.gameObject.SetActive(true);
    }

    public void Open(GameObject clickObj)
    {
        rectTransform.anchoredPosition = clickObj.transform.ConverToWorldPoint(rectTransform.parent as RectTransform, Vector2.zero);
        targetObj = clickObj.GetComponent<LauncherTower>();
        gameObject.SetActive(true);
    }

    public void Close()
    {
        targetObj = null;
    }

    private void OnEnable()
    {
    }

    private void OnDisable()
    {
        launchIndicatorLine.gameObject.SetActive(false);
    }

    private void Update()
    {
        switch (stateMode)
        {
            case 1://瞄准模式
                AimDiraction();
                break;
            default:
                break;
        }
    }

    //Tower function
    public void AimDiraction()
    {
        bool haveHit;
        // 鼠标地面检测
        var physicsWorldSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<Unity.Physics.Systems.BuildPhysicsWorld>();
        var collisionWorld = physicsWorldSystem.PhysicsWorld.CollisionWorld;
        Vector2 mousePosition = Input.mousePosition;
        UnityEngine.Ray unityRay = Camera.main.ScreenPointToRay(mousePosition);
        var rayInput = new RaycastInput
        {
            Start = unityRay.origin,
            End = unityRay.origin + unityRay.direction * 100f,
            Filter = new CollisionFilter
            {
                BelongsTo = ~0u,
                CollidesWith = 1u << 11,//地面层
                GroupIndex = 0
            }
        };

        haveHit = collisionWorld.CastRay(rayInput, out var hit);

        if (haveHit)
        {
            launchIndicatorLine.SetLineIndection(targetObj.transform.position, hit.Position);
            targetObj.SetLaunchDirction(hit.Position);
        }

        if (Input.GetMouseButtonDown(0))
        {
            isLeftButtonPress = true;
        }
        if (isLeftButtonPress)
        {//确认设置发射方向
            if (Input.GetMouseButtonUp(0))
            {
                targetObj.ApplyDiraction();
                stateMode = 0;
                isLeftButtonPress = false;
                launchIndicatorLine.gameObject.SetActive(false);
                gameObject.SetActive(false);
            }
        }
    }
}
