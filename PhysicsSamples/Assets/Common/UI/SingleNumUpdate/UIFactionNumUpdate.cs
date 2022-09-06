using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UIFactionNumUpdate : MonoBehaviour
{
    //
    [SerializeField] Vector2IntEventChannelSO numEvent;
    [SerializeField] TextMeshProUGUI numText;
    private void OnEnable()
    {
        numEvent.OnEventRaised += OnEventRais;
    }

    private void OnDisable()
    {
        numEvent.OnEventRaised -= OnEventRais;
    }

    void OnEventRais(Vector2Int ab)
    {
        var a = ab.x;
        var b = ab.y;
        numText.text =  $"{a}/{b}";
    }
}
