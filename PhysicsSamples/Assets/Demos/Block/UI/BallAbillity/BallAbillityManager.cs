using DG.Tweening;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

public class BallAbillityManager : MonoBehaviour
{
    List<Entity> gunEnties;
    [SerializeField] Dictionary<int, UnityAction> AbFunctionMap = new Dictionary<int, UnityAction>();
    void Start()
    {
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
    [SerializeField] List<RpgEffectSO> allSkillSO;
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
        Expand(true);
    }

    //Unity.Core.TimeData time = new Unity.Core.TimeData();
    //打开ui ,带功能性的更新
    public void OpenPanel()
    {
        Expand(true);
    }

    //提交并关闭ui
    public void Submit(RpgEffectSO rpgEffectSO, int rank)
    {
        PlayerEcsConnect.Instance.AddBuff(rpgEffectSO, rank);
    }

    //显示ui
    public void Expand(bool opt)
    {
        if (opt)
        {
            Time.timeScale = 0.05f;
            canvasGroup.alpha = 1;
            canvasGroup.interactable = true;
            //DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 1, 0.5f).SetUpdate(true);
            foreach (var item in allBallBuffCardUI)
            {
                item.transform.DOScale(1, 0.3f).From(0.5f).SetEase(Ease.OutCubic).SetUpdate(true);
            }
        }
        else
        {
            Time.timeScale = 1;

            canvasGroup.alpha = 0;
            canvasGroup.interactable = false;
            //DOTween.To(() => canvasGroup.alpha, x => canvasGroup.alpha = x, 0, 0.5f);
        }
    }

    //收起ui
    public void Fold()
    {
    }
}
