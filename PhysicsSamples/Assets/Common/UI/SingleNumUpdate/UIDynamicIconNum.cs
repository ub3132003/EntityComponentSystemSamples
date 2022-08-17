using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UIDynamicIconNum : MonoBehaviour
{
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI num;

    //[SerializeField] IconNumChannelSO ItemEvent;

    //private void OnEnable()
    //{
    //    ItemEvent.OnEventRaised += SetItem;
    //}

    //private void OnDisable()
    //{
    //    ItemEvent.OnEventRaised -= SetItem;
    //}

    public void SetItem(Sprite icon, int amount)
    {
        Icon.sprite = icon;
        num.text = amount.ToString();
    }
}
