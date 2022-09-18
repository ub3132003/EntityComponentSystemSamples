using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;
using Sirenix.OdinInspector;

/// <summary>
/// 对自己释放技能
/// </summary>

public class BallBuffCardUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button CardButton;
    //整卡背景
    [SerializeField] Image BG;
    [SerializeField] Image Icon;
    //图标背景,职业
    [SerializeField] Image IconBGI;
    //稀有度
    [SerializeField] Image RaceImage;
    [SerializeField] TextMeshProUGUI title;
    [SerializeField] TextMeshProUGUI description;
    [SerializeField] Image disableCover;
    public int CardId;

    public Color GetColor(int i)
    {
        List<Color> BG_COLOR = new List<Color>(3);
        Color c;
        ColorUtility.TryParseHtmlString("#E3E3E3", out c);
        BG_COLOR.Add(c);
        ColorUtility.TryParseHtmlString("#2980B9", out c);
        BG_COLOR.Add(c);
        ColorUtility.TryParseHtmlString("#F1C40F", out c);
        BG_COLOR.Add(c);

        return BG_COLOR[i];
    }

    public void SetCard(string title, string detail, Sprite icon, int rank)
    {
        this.title.text = title;
        description.text = detail;
        Icon.sprite = icon;
        RaceImage.color = GetColor(rank);
    }

    public void SetIntactionable(bool opt)
    {
        CardButton.interactable = opt;
        disableCover.enabled = !opt;
    }

    [Button]
    public void SetNull(bool opt)
    {
        if (title != null) title.enabled = opt;
        if (description != null) description.enabled = opt;
        Icon.enabled = opt;
        RaceImage.enabled = opt;
    }

    public void SetBackGroudColor(Color color)
    {
        BG.color = color;
    }

    /// <summary>
    /// 点击卡牌时执行
    /// </summary>
    public UnityAction SubmitAction;
    private void Awake()
    {
        CardButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        SubmitAction?.Invoke();
    }

    Tweener tweener;
    public void OnPointerEnter(PointerEventData eventData)
    {
        //transform.DOScale(1.1f, 0.2f).SetUpdate(true);
        transform.localScale = Vector3.one * 1.2f;
        tweener?.Kill();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        tweener = transform.DOScale(1.0f, 0.5f).SetUpdate(true);
    }
}
