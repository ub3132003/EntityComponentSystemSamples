using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallAbillityHadUIPanel : MonoBehaviour
{
    [SerializeField] PanelBallSelectAb panelBallAb;
    List<BallAbillityMap> hadCard => panelBallAb.AllHadAbillity;
    void Start()
    {
        oldHadAbillityCount = hadCard.Count;
    }

    int oldHadAbillityCount = 0;
    void Update()
    {
        if (oldHadAbillityCount != hadCard.Count)
        {
            oldHadAbillityCount = hadCard.Count;

            UpdateHadAbillity();
        }
    }

    //buff得数值
    [SerializeField] IntEventChannelSO BuffValueEvent;
    [SerializeField] GameObject HadAbillityUIPrefab;
    List<UIDynamicIconNum> uIIcons = new List<UIDynamicIconNum>();

    //只会增加
    void UpdateHadAbillity()
    {
        var length = hadCard.Count;
        for (int i = 0; i < length; i++)
        {
            var cardData = hadCard[i];

            UIDynamicIconNum uiItem;
            if (i < uIIcons.Count)
            {
                //显示卡
                uiItem = uIIcons[i];
            }
            else
            {
                uiItem = Instantiate(HadAbillityUIPrefab, transform).GetComponent<UIDynamicIconNum>();
                uIIcons.Add(uiItem);
                //uIIcons.Add
            }
            uiItem.SetItem(cardData.cardRef.Name.GetLocalizedString(), cardData.cardRef.Value);
        }
    }
}
