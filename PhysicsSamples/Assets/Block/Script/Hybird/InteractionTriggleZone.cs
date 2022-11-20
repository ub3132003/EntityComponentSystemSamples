using System.Collections;
using UnityEngine;
using UnityEngine.Events;

namespace Assets.Block.Script.Hybird
{
    public class InteractionTriggleZone : MonoBehaviour, IInteraction
    {
        public int PlayerInteraction(Transform player)
        {
            ActionEvent.Invoke(player);
            return 0;
        }

        public UnityEvent<Transform> ActionEvent;

        /// <summary>
        /// 进入事件1
        /// 退出事件0
        /// </summary>
        [SerializeField] IntGameObjectEventChannelSO InteractionUICueEvent;

        private void OnTriggerEnter(Collider other)
        {
            if (other.tag == "Player")
            {
                InteractionUICueEvent.RaiseEvent(1, this.gameObject);
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.tag == "Player")
            {
                InteractionUICueEvent.RaiseEvent(0, this.gameObject);
            }
        }
    }
}
