using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 购买补充球
/// </summary>
public class BuyBallStore : MonoBehaviour, IInteraction
{
    public int PlayerInteraction(Transform player)
    {
        BallAbillityManager.Instance.BuyAllBullet();
        return 0;
    }

    [SerializeField] IntGameObjectEventChannelSO EnterStoreEvent;

    private void OnTriggerEnter(Collider other)
    {
        if (other.tag == "Player")
        {
            EnterStoreEvent.RaiseEvent(1, this.gameObject);
        }
    }

    private void OnTriggerExit(Collider other)
    {
        if (other.tag == "Player")
        {
            EnterStoreEvent.RaiseEvent(0, this.gameObject);
        }
    }
}
