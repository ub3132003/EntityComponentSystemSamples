using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Assertions;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

/// <summary>
/// 对自己释放技能
/// </summary>

public class UpdateSkillSelectUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [SerializeField] Button CardButton;
    [SerializeField] Image Icon;
    [SerializeField] Image IconBGI;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] TextMeshProUGUI description;

    class SkillData
    {
        public RpgEffectSO skillRef;
        public int rank;
    }

    SkillData skillData = new SkillData();

    public Color GetColor(int i)
    {
        List<Color> BG_COLOR = new List<Color>(3);
        Color c;
        ColorUtility.TryParseHtmlString("#BDC3C7", out c);
        BG_COLOR.Add(c);
        ColorUtility.TryParseHtmlString("#2980B9", out c);
        BG_COLOR.Add(c);
        ColorUtility.TryParseHtmlString("#F1C40F", out c);
        BG_COLOR.Add(c);

        return BG_COLOR[i];
    }

    public void SetCard(string text, Sprite icon , int rank)
    {
        description.text = text;
        Icon.sprite = icon;
        IconBGI.color = GetColor(rank)
        ;
        skillData.rank = rank;
    }

    public UnityAction SubmitAction;
    private void Awake()
    {
        CardButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        //OnSubmit.Invoke(skillData.skillRef, skillData.rank);
        SubmitAction.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.DOScale(1.1f, 0.2f).SetUpdate(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOScale(1.0f, 0.2f).SetUpdate(true);
    }
}
