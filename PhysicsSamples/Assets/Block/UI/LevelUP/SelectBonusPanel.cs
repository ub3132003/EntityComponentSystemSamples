using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Unity.Entities;
using DataStructures.RandomSelector;

public class SelectBonusPanel : MonoBehaviour
{
    [SerializeField] IntEventChannelSO updateEvent;
    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] List<UIBonusSkillCard> allSkillCardUI;
    [SerializeField] List<RPGBonus> allBonusSO;
    private void OnEnable()
    {
        updateEvent.OnEventRaised += LevelUPDo;
    }

    private void OnDisable()
    {
        updateEvent.OnEventRaised -= LevelUPDo;
    }

    private void Start()
    {
        OpenPanel(false);
    }

    //Unity.Core.TimeData time = new Unity.Core.TimeData();
    public void OpenPanel(bool opt)
    {
        if (opt)
        {
            Time.timeScale = 0.05f;
            canvasGroup.alpha = 0;
            canvasGroup.interactable = true;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.5f).SetUpdate(true);
        }
        else
        {
            Time.timeScale = 1;

            canvasGroup.alpha = 1;
            canvasGroup.interactable = false;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, 0.5f);
        }
    }

    void LevelUPDo(int level)
    {
        OpenPanel(true);

        GetRandomSkill();
    }

    public float[] DropRate = { 0.6f, 0.3f, 0.1f };
    public void GetRandomSkill()
    {
        var length = allSkillCardUI.Count;

        //洗牌抽取
        var randomSkill = allBonusSO.OrderBy(d => Random.Range(0, 100)).Take(length);
        var i = 0;

        foreach (var item in randomSkill)
        {
            var rank = CharacterData.GetRankFromCharacterData(item);
            //找到了 第一级
            if (rank == -1)
            {
                rank = 0;
            }//已近有的 加一级
            else
            {
                rank++;
            }
            var ui = allSkillCardUI[i++];
            ui.SetCard(item.Name.GetLocalizedString(), item.PreviewImage , rank + 1);
            ui.SubmitAction = () => Submit(item);
        }
    }

    public void Submit(RPGBonus rPGBonus)
    {
        //升一级被动技能
        BonusManager.RankUpBonus(rPGBonus);
    }
}
