using DG.Tweening;
using Sirenix.OdinInspector;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class BallAbillityManager : SerializedMonoBehaviour
{
    List<Entity> gunEnties;
    [System.Serializable]
    class BallAbillityMap
    {
        public BallBuffCardSO cardRef;
        public UnityEvent<int> cardFunc;

        public void CallFunc()
        {
            cardFunc?.Invoke(cardRef.Value);
        }
    }
    [TableList(ShowIndexLabels = true)]
    [SerializeField] List<BallAbillityMap> AbillityList;

    [SerializeField] Dictionary<int, UnityAction> AbFunctionMap = new Dictionary<int, UnityAction>();
    void Start()
    {
        Expand(false);

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
        em = PlayerEcsConnect.Instance.EntityManager;


        //map
        AbFunctionMap.Add(1, () => AddDamage(1));
    }

    EntityManager em;

    public void CallFuncMap(int id)
    {
        AbFunctionMap.TryGetValue(id, out var func);
        func.Invoke();
    }

    public void AddDamage(int val)
    {
        foreach (var item in gunEnties)
        {
            var ball = em.GetComponentData<CharacterGun>(item).Bullet;
            var damage = em.GetComponentData<Damage>(ball);
            damage.DamageValue++;
            em.SetComponentData(ball, damage);
        }
    }

    public void AddAbAddDamageOnCatch(int val)
    {
        foreach (var item in gunEnties)
        {
            var ball = em.GetComponentData<CharacterGun>(item).Bullet;
            em.AddComponentData(ball, new DamageAddOnCatchTag());
        }
    }

    [SerializeField] IntEventChannelSO openPanelEvent;
    [SerializeField] CanvasGroup canvasGroup;

    [SerializeField] List<BallBuffCardUI> allBallBuffCardUI;
    [SerializeField] List<BallBuffCardSO> allSkillSO;
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

    //Unity.Core.TimeData time = new Unity.Core.TimeData();
    //打开ui ,带功能性的更新
    public void OpenPanel()
    {
        Expand(true);

        //填充卡片
        foreach (var item in allBallBuffCardUI)
        {
            var cardRef = AbillityList[0].cardRef;
            item.SetCard(cardRef.Description.GetLocalizedString(), cardRef.PreviewImage, 0);
            item.SubmitAction = AbillityList[0].CallFunc;
        }
    }

    //提交并关闭ui
    public void Submit(RpgEffectSO rpgEffectSO, int rank)
    {
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
