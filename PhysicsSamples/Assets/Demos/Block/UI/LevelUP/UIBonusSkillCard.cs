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

public class UIBonusSkillCard : MonoBehaviour
{
    [SerializeField] Button CardButton;
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI Lv;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] TextMeshProUGUI description;


    public void SetCard(string text, Sprite icon , int rank)
    {
        description.text = text;
        Icon.sprite = icon;
        Lv.text = rank.ToString();
    }

    public UnityAction SubmitAction;
    private void Awake()
    {
        CardButton.onClick.AddListener(OnClick);
    }

    public void OnClick()
    {
        SubmitAction.Invoke();
    }
}
