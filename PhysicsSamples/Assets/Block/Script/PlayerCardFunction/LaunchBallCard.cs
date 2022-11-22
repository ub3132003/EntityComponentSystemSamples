using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Physics;
using UnityEngine.InputSystem;

public class LaunchBallCard : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetMouseButton(0))
        {
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

            bool haveHit = collisionWorld.CastRay(rayInput, out var hit);

            if (haveHit)
            {
                launchIndicatorLine.SetLineIndection(hit.Position);
                SetLaunchDirction(hit.Position);
            }

            SetLaunchIndicator(true);
        }

        if (Input.GetMouseButtonUp(0))
        {
            CardAction();
        }
    }

    // 拖拽卡牌到激活区，
    //提示弹道,
    //松开鼠标确定弹道
    //回合结束时发出球.

    [SerializeField] launchIndicatorLine launchIndicatorLine;
    //发射方向
    public Vector3 LaunchDir = Vector3.zero;
    public ThingSO BallSO;
    public void SetLaunchDirction(Vector3 endPosition)
    {
        var playPosition = PlayerEcsConnect.Instance.GetPlayerPosition();
        var targetDir = endPosition - playPosition;
        LaunchDir = targetDir;

        //设置实体方向
        PlayerEcsConnect.Instance.RotatePlayerTo(LaunchDir);
    }

    //显示指示器hud
    public void SetLaunchIndicator(bool opt)
    {
        launchIndicatorLine.gameObject.SetActive(opt);
    }

    public void LaunchBall(int Amount)
    {
        //找到实体
        //添加子弹

        //发射实体

        BallAbillityManager.Instance.ChangeGun(BallSO);
        BallAbillityManager.Instance.AddBall(BallSO, Amount);
        BallAbillityManager.Instance.SetFireGun(BallSO);
    }

    public void CardAction()
    {
        LaunchBall(3);
    }
}
