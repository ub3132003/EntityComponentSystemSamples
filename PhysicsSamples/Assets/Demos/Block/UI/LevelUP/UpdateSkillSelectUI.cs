using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Unity.Assertions;
using UnityEngine.Events;

/// <summary>
/// 对自己释放技能
/// </summary>

public class UpdateSkillSelectUI : MonoBehaviour
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

        public SkillData(RpgEffectSO skillRef, int rank)
        {
            this.skillRef = skillRef;
            this.rank = rank;
        }
    }
    SkillData skillData;


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

    public void SetCard(RpgEffectSO cardSO , int rank)
    {
        description.text = cardSO.Description.GetLocalizedString();
        Icon.sprite = cardSO.PreviewImage;
        IconBGI.color = GetColor(rank);
        skillData =  new SkillData(cardSO, rank);

        Assert.AreEqual(skillData.skillRef.effectType, RpgEffectSO.EFFECT_TYPE.Stat , "Only Stat Allowd");
    }

    public UnityEvent<RpgEffectSO , int> OnSubmit;
    private void Awake()
    {
        CardButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        OnSubmit.Invoke(skillData.skillRef, skillData.rank);
    }
}
