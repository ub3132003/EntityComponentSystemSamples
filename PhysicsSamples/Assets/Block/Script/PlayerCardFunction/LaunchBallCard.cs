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

    private void OnEnable()
    {
        SetLaunchIndicator(true);
    }

    private void OnDisable()
    {
        SetLaunchIndicator(false);
    }

    /// <summary>
    ///
    /// </summary>
    /// <param name="viladArea">打牌区 </param>
    /// <param name="launchIndicatorLine">指示方向 </param>
    public void Init(RectTransform viladArea, LaunchIndicatorLine launchIndicatorLine)
    {
        var uiDrag = gameObject.GetComponent<UIDrag>();
        uiDrag.validArea.Clear();
        uiDrag.validArea.Add(viladArea);
        this.launchIndicatorLine = launchIndicatorLine;
    }

    // Update is called once per frame
    void Update()
    {
        bool haveHit;

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

            haveHit = collisionWorld.CastRay(rayInput, out var hit);

            if (haveHit)
            {
                launchIndicatorLine.SetLineIndection(hit.Position);
                SetLaunchDirction(hit.Position);
            }
            else
            {
                launchDir = Vector3.zero;
            }
        }

        if (Input.GetMouseButtonUp(0) && launchDir != Vector3.zero)
        {
            LaunchBall(3);
            Destroy(this.gameObject, 1f);
        }
    }

    // 打出卡牌切换子弹类型，
    //提示进入发射模式，引导发射操作
    //松开鼠标确定弹道，发出球.


    [SerializeField] LaunchIndicatorLine launchIndicatorLine;
    //发射方向
    private Vector3 launchDir = Vector3.zero;
    public ThingSO BallSO;

    public Vector3 LaunchDir { get => launchDir;}

    public void SetLaunchDirction(Vector3 endPosition)
    {
        var playPosition = PlayerEcsConnect.Instance.GetPlayerPosition();
        //不考虑仰角
        endPosition.y = playPosition.y;
        var targetDir = endPosition - playPosition;
        launchDir = targetDir;

        //设置实体方向
        PlayerEcsConnect.Instance.RotatePlayerTo(launchDir);
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

        //BallAbillityManager.Instance.AddBall(BallSO, Amount);
        BallAbillityManager.Instance.SetFireGun(BallSO);
    }

    public void CardAction()
    {
        BallAbillityManager.Instance.ChangeGun(BallSO);
    }

    public static void LaunchBall(ThingSO ballSO, int amount)
    {
        BallAbillityManager.Instance.AddBall(ballSO, amount);
        BallAbillityManager.Instance.SetFireGun(ballSO);
    }
}
