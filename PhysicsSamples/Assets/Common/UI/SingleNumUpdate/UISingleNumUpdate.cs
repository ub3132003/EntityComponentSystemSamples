using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
public class UISingleNumUpdate : MonoBehaviour
{
    //
    [SerializeField] IntEventChannelSO numEvent;
    [SerializeField] TextMeshProUGUI numText;
    private void OnEnable()
    {
        numEvent.OnEventRaised += OnEventRais;
    }

    private void OnDisable()
    {
        numEvent.OnEventRaised -= OnEventRais;
    }

    void OnEventRais(int n)
    {
        numText.text = n.ToString();
    }
}
