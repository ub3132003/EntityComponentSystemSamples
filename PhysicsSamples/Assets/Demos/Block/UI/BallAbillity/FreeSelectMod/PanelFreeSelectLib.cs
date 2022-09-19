using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class PanelFreeSelectLib : MonoBehaviour
{
    List<BallData> cardPool;
    [SerializeField] GameObject cardPrefab;
    /// <summary>
    /// 备选的所有卡片父对象
    /// </summary>
    [SerializeField] GameObject libCardContent;

    [SerializeField] Button SumbitButton;
    /// <summary>
    /// 可选择球的数量
    /// </summary>
    public int MaxSelectNum = 0;
    void Start()
    {
        cardPool = new List<BallData>(BallAbillityManager.Instance.BallDataList);

        for (int i = 0; i < cardPool.Count; i++)
        {
            var cardUIObj = Instantiate(cardPrefab, libCardContent.transform);
            var cardUI = cardUIObj.GetComponent<ICardUI>();
            BallAbillityManager.Instance.FillBallCard(cardUI , i);
            cardUI.CardId = i;
            cardUIObj.GetComponent<BallCardUI>().OnValueChanged = SelectCard;
        }

        SumbitButton.onClick.AddListener(Submit);
    }

    public UnityAction<List<ThingSO>> SubmitAction;
    public void Submit()
    {
        var selectedCards = BallAbillityManager.Instance.BallSelectedContent.GetComponentsInChildren<ICardUI>();

        //for (int i = 0; i < selectedCards.Length; i++)
        //{
        //    var card = selectedCards[i];

        //}
        var ballInfos = selectedCards.Select(card => cardPool[card.CardId].BallUI).ToList();
        SubmitAction?.Invoke(ballInfos);
    }

    private int selectedCount = 0;
    //选取卡片备战，注册到toggle上
    public void SelectCard(bool isOn)
    {
        if (isOn)
        {
            selectedCount++;
        }
        else
        {
            selectedCount--;
        }

        var isFullCardSolt = selectedCount == MaxSelectNum;
        //选满才能提交
        SumbitButton.interactable = isFullCardSolt;

        var toggles = libCardContent.GetComponentsInChildren<Toggle>();
        if (isFullCardSolt)
        {
            foreach (var item in toggles)
            {
                item.interactable = false;
            }
        }
        else
        {
            foreach (var item in toggles)
            {
                item.interactable = true;
            }
        }
    }
}
