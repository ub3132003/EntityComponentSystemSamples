using System.Collections;
using System.Collections.Generic;
using UnityEngine;
public interface IInteraction
{
    /// <summary>
    /// 玩家进行交互，按交互键触发
    /// </summary>
    /// <returns></returns>
    public int PlayerInteraction(Transform player);
}

/// <summary>
/// 转换当前的球
/// </summary>
public class BallChangeInteraction : MonoBehaviour, IInteraction
{
    [SerializeField] IntGameObjectEventChannelSO changeBallReadyEvent;
    [SerializeField] ThingSO BallItem;
    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            changeBallReadyEvent.RaiseEvent(1, this.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            changeBallReadyEvent.RaiseEvent(0, this.gameObject);
        }
    }

    public int PlayerInteraction(Transform player)
    {
        Debug.Log("交互开始");
        BallAbillityManager.Instance.ChangeGun(BallItem);
        return 0;
    }
}
