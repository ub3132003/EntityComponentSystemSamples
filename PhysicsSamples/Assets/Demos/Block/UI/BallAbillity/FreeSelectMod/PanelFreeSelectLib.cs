using System.Collections;
using System.Collections.Generic;
using UnityEngine;
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
    void Start()
    {
        cardPool = new List<BallData>(BallAbillityManager.Instance.BallDataList);

        for (int i = 0; i < cardPool.Count; i++)
        {
            var cardUIObj = Instantiate(cardPrefab, libCardContent.transform);
            var cardUI = cardUIObj.GetComponent<ICardUI>();
            BallAbillityManager.Instance.FillBallCard(cardUI , i);

            cardUIObj.GetComponent<BallCardUI>().OnValueChanged = SelectCard;
        }
    }

    // Update is called once per frame
    void Update()
    {
    }

    List<BallAbillityMap> selectedCardList;
    public int selectedCount = 0;
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

        var isFullCardSolt = selectedCount >= 4;

        //选满才能提交
        SumbitButton.interactable = isFullCardSolt;
    }
}
