using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using Unity.Assertions;
using Unity.Entities;
using DG.Tweening;

public class PanelBallSelectAb : MonoBehaviour
{
    [SerializeField] IntEventChannelSO openPanelEvent;
    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] List<BallBuffCardUI> allBallBuffCardUI;
    [SerializeField] List<Color> allColor;

    List<Entity> gunEnties;
    private void Start()
    {
        Expand(false);
        cardPool = new List<BallAbillityMap>(BallAbillityManager.Instance.AbillityList);

        //查找所有gun 实体
        var gunSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<CharacterGunOneToManyInputSystem>();
        EntityQueryDesc description = new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(CharacterGun),
            }
        };
        var queryBuilder = new EntityQueryDescBuilder(Unity.Collections.Allocator.Temp);
        queryBuilder.AddAll(typeof(CharacterGun));
        queryBuilder.FinalizeQuery();

        EntityQuery gunGroup = gunSystem.GetEntityQuery(queryBuilder);

        var guns = gunGroup.ToEntityArray(Unity.Collections.Allocator.Temp);
        gunEnties = new List<Entity>(guns);

        queryBuilder.Dispose();
        guns.Dispose();
    }

    private void OnEnable()
    {
        openPanelEvent.OnEventRaised += open;
    }

    private void OnDisable()
    {
        openPanelEvent.OnEventRaised -= open;
    }

    void open(int t)
    {
        OpenPanel();
    }

    //抽取能力的池子，从游戏开始时算起
    List<BallAbillityMap> cardPool;
    List<GameObject> BallPrefabs => BallAbillityManager.Instance.BallPrefabs;

    //Unity.Core.TimeData time = new Unity.Core.TimeData();
    //打开ui ,带功能性的更新
    public void OpenPanel()
    {
        Expand(true);


        List<BallAbillityMap> takeCardList = new List<BallAbillityMap>();
        int randomIdx = 0;
        //TOdo 单个类别中卡组不够填充 选项。
        for (int i = 0; i < 3; i++)
        {
            //取类别,利用指定位置刷新来解决不同类别抽取
            randomIdx = Random.Range(0, BallPrefabs.Count);
            var ballCategoryId = BallPrefabs[randomIdx].GetInstanceID();
            takeCardList = cardPool.Where((x) => x.CategoryId == ballCategoryId).ToList();
            if (takeCardList.Count >= allBallBuffCardUI.Count)
            {
                break;
            }
        }
        if (takeCardList.Count < allBallBuffCardUI.Count)
        {
            Debug.LogError("此种球升级备选卡片不够");
            return;
        }
        var BgColor = allColor[randomIdx];
        //填充卡片
        foreach (var item in allBallBuffCardUI)
        {
            //取球能力

            randomIdx = Random.Range(0, takeCardList.Count);
            var abCard = takeCardList[randomIdx];

            item.SetBackGroudColor(BgColor);
            item.SetCard(abCard.cardRef.Name.GetLocalizedString(), abCard.cardRef.Description.GetLocalizedString(), abCard.cardRef.PreviewImage, 0);
            item.SubmitAction = () => Submit(abCard);
            item.CardId = randomIdx;

            //不重复抽取
            takeCardList.RemoveAt(randomIdx);
        }
    }

    List<BallAbillityMap> allHadAbillity;
    public List<BallAbillityMap> AllHadAbillity
    {
        get
        {
            if (allHadAbillity == null)
            {
                allHadAbillity = new List<BallAbillityMap>();
            }
            return allHadAbillity;
        }
    }
    //提交并关闭ui
    public void Submit(BallAbillityMap card)
    {
        var em = PlayerEcsConnect.Instance.EntityManager;
        foreach (var item in gunEnties)
        {
            var gun = em.GetComponentData<CharacterGun>(item);
            if (gun.ID != card.CategoryId)
            { continue; }

            //添加到对应球
            card.CallFunc(item);
            if (card.cardRef.isUnique)
            {
                cardPool.Remove(card);
            }
            allHadAbillity.Add(card);
        }
        Expand(false);
    }

    //显示ui     //收起ui
    public void Expand(bool opt)
    {
        if (opt)
        {
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
            foreach (var item in allBallBuffCardUI)
            {
                item.transform.DOScale(1, 0.3f).From(0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }
        else
        {
            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            canvasGroup.blocksRaycasts = false;
            //DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, 0.5f);
        }
    }
}
