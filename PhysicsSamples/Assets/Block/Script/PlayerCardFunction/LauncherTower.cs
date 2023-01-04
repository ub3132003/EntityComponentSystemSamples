using System.Collections;
using System.Collections.Generic;
using Unity.Transforms;
using Unity.Entities;
using UnityEngine;
using Unity.Mathematics;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
public class LauncherTower : MonoBehaviour, IReceiveEntity
{
    //发射方向
    private Vector3 launchDir = Vector3.zero;
    //发射球类型
    [SerializeField] ThingSO BallSO;
    [SerializeField] LaunchIndicatorLine launchIndicatorLine;

    //boradcast on
    //[SerializeField] GameObjectEventChannelSO cilckEvent;
    private Entity followEntity;

    public GameObject EntityPerfab;
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
        quaternion rotation = quaternion.LookRotationSafe(launchDir, math.up());
        PlayerEcsConnect.Instance.EntityManager.SetComponentData<Rotation>(followEntity, new Rotation
        {
            Value = rotation
        });
    }

    #region interface
    public void SetReceivedEntity(Entity entity)
    {
        followEntity = entity;
    }

    //public void OnPointerClick(PointerEventData eventData)
    //{
    //    stateMode = 1;
    //    cilckEvent.RaiseEvent(this.gameObject);
    //}

    #endregion
}
