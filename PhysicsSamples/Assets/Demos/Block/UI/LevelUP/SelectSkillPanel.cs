using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Linq;

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

    public void OpenPanel(bool opt)
    {
        if (opt)
        {
            canvasGroup.alpha = 0;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.5f);
        }
        else
        {
            canvasGroup.alpha = 1;
            DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, 0.5f);
        }
    }

    void LevelUPDo(int level)
    {
        OpenPanel(true);

        GetRandomSkill();
    }

    public void GetRandomSkill()
    {
        var length = allSkillCardUI.Count;

        //洗牌抽取
        var randomSkill = allSkillSO.OrderBy(d => Random.Range(0, 100)).Take(length);
        var i = 0;
        foreach (var item in randomSkill)
        {
            allSkillCardUI[i++].SetCard(item);
        }
    }

    public void Submit()
    {
    }
}
