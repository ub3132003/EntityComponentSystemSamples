using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class CardManager : MonoBehaviour
{
    [SerializeField] List<GameObject> cardPerfabs;
    [SerializeField] Transform Content;
    [SerializeField] LaunchIndicatorLine launchIndicatorLine;
    private void Start()
    {
        Submit.onClick.AddListener(() => CreateCard(dropdownCardCreate.value));
    }

    [Header("DEBUg")]
    [SerializeField] TMP_Dropdown dropdownCardCreate;
    [SerializeField] Button Submit;
    //下拉序号创建卡牌.debug用
    public void CreateCard(int index)
    {
        var card = Instantiate(cardPerfabs[index], Content);
    }
}
