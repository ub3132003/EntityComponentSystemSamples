using System.Collections;
using System.Collections.Generic;
using Unity.Assertions;
using UnityEngine;

public class BuffListInfoUI : MonoBehaviour
{
    //public List<IconNumChannelSO> buffDisplayEvent;
    public List<UIDynamicIconNum> buffSlots;
    public CombatNode combatNode;

    private void Start()
    {
        combatNode = PlayerEcsConnect.Instance.PlayerNode;
        buffSlots = new List<UIDynamicIconNum>(GetComponentsInChildren<UIDynamicIconNum>());
        foreach (var item in buffSlots)
        {
            item.gameObject.SetActive(false);
        }
    }

    void Update()
    {
        var length = combatNode.nodeStateData.Count;
        var soltCount = buffSlots.Count;
        Assert.IsTrue(soltCount >= length);

        for (int i = 0; i < length; i++)
        {
            var nodeState = combatNode.nodeStateData[i];
            //buffDisplayEvent[i].RaiseEvent(nodeState.stateIcon, nodeState.curStack);
            buffSlots[i].gameObject.SetActive(true);
            buffSlots[i].SetItem(nodeState.stateIcon, nodeState.curStack);
        }

        for (int i = length; i < soltCount - length; i++)
        {
            buffSlots[i].gameObject.SetActive(false);
        }
    }
}
