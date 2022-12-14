using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 少量情况的 进度条类ui
/// </summary>
public class ProgressBar : MonoBehaviour
{
    Image _barFill;
    [SerializeField] FloatEventChannelSO _barUpdateEvent;
    //daoshu
    public bool OneMinus;
    //TODO 分数版本
    private void Awake()
    {
        _barFill = GetComponent<Image>();
    }

    private void OnEnable()
    {
        _barUpdateEvent.OnEventRaised += UpdateBar;
    }

    private void OnDisable()
    {
        _barUpdateEvent.OnEventRaised -= UpdateBar;
    }

    private void UpdateBar(float fillAmount)
    {
        if (OneMinus) { fillAmount = 1 - fillAmount; }
        _barFill.fillAmount = fillAmount;
    }
}
