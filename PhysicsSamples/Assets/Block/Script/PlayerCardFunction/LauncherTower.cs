using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.EventSystems;

public class LauncherTower : MonoBehaviour, IReceiveEntity, IPointerClickHandler
{
    //发射方向
    private Vector3 launchDir = Vector3.zero;
    //发射球类型
    [SerializeField] ThingSO BallSO;
    [SerializeField] LaunchIndicatorLine launchIndicatorLine;

    private Entity followEntity;


    private int stateMode;
    public void SetLaunchDirction(Vector3 endPosition)
    {
        var playPosition = this.transform.position;
        //不考虑仰角
        endPosition.y = playPosition.y;
        var targetDir = endPosition - playPosition;
        launchDir = targetDir;
    }

    public void ApplyDiraction()
    {
        //设置实体方向
        quaternion rotation = quaternion.LookRotation(launchDir, math.up());
        PlayerEcsConnect.Instance.EntityManager.SetComponentData<Rotation>(followEntity, new Rotation
        {
            Value = rotation
        });
    }

    public void AimDiraction()
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
            ApplyDiraction();
            stateMode = 0;
            this.enabled = false;
        }
    }

    private void OnEnable()
    {
        launchIndicatorLine.gameObject.SetActive(true);
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

    private void Start()
    {
    }

    #region interface
    public void SetReceivedEntity(Entity entity)
    {
        followEntity = entity;
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        stateMode = 1;
    }

    #endregion
}
