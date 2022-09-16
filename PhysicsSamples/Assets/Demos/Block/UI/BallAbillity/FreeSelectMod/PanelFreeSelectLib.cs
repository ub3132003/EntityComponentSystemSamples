using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PanelFreeSelectLib : MonoBehaviour
{
    List<BallAbillityMap> cardPool;
    [SerializeField] BallBuffCardUI cardPrefab;
    /// <summary>
    /// 备选的所有卡片父对象
    /// </summary>
    [SerializeField] GameObject libCardContent;
    void Start()
    {
        cardPool = new List<BallAbillityMap>(BallAbillityManager.Instance.AbillityList);

        for (int i = 0; i < cardPool.Count; i++)
        {
            var cardUI = Instantiate(cardPrefab, libCardContent.transform);
            BallAbillityManager.Instance.FillAbCard(cardUI, i);
            cardUI.SubmitAction = () => cardUI.SetIntactionable(false);
        }
    }

    // Update is called once per frame
    void Update()
    {
    }
}
