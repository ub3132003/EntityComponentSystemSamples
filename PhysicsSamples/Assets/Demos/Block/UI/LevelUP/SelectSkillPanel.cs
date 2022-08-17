using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;
using Unity.Entities;

public class SelectSkillPanel : MonoBehaviour
{
    [SerializeField] IntEventChannelSO updateEvent;
    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] List<UpdateSkillSelectUI> allSkillCardUI;
    [SerializeField] List<RpgEffectSO> allSkillSO;
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

    void Update()
    {
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
        var randomSkill = allSkillSO.OrderBy(d => Random.Range(0, 100)).Take(length);
        var i = 0;

        foreach (var item in randomSkill)
        {
            var rank = StepRandom(DropRate);
            if (rank > item.ranks.Count - 1)//防止越界
            {
                rank = item.ranks.Count - 1;
            }
            allSkillCardUI[i++].SetCard(item , rank);
        }
    }

    /// <summary>
    /// 返回 0-... 按给定概率 需要保证rate和=1;
    /// </summary>
    /// <param name="rate"> 0-1, 和=1</param>
    /// <returns></returns>
    public int StepRandom(float[] rate)
    {
        var sumRate = 0f;
        for (int i = 0; i < rate.Length; i++)
        {
            sumRate += rate[i];
            if (sumRate > 1)
            {
                break;
            }
            if (Random.value < sumRate)
            {
                return i;
            }
        }
        return 0;
    }

    public void Submit()
    {
    }
}
