using Sirenix.OdinInspector;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 附加能力到球上
/// </summary>
[System.Serializable]
public class BallAbillityMap
{
    /// <summary>
    /// 限定作用的球
    /// </summary>
    ///
    [ValueDropdown("@BallAbillityManager.Instance.BallPrefabs")]
    [SerializeField] GameObject TargetBall;

    public BallBuffCardSO cardRef;
    public UnityEvent<Entity, int> cardFunc;

    public int CategoryId => TargetBall.GetInstanceID();
    public void CallFunc(Entity gun)
    {
        cardFunc?.Invoke(gun, cardRef.Value);
    }
}
/// <summary>
/// 球的信息
/// </summary>
[System.Serializable]
public class BallData
{
    public ThingSO BallUI;
    //单个球花费
    public int Price;
}

/// <summary>
/// 球能力管理器
/// </summary>
public class BallAbillityManager : Singleton<BallAbillityManager>
{
    [TabGroup("BallAb")]
    [AssetsOnly]
    public List<GameObject> BallPrefabs;

    [TabGroup("BallAb")]
    [TableList(ShowIndexLabels = true)]
    [SerializeField]
    public List<BallAbillityMap> AbillityList;

    [TabGroup("BallData")]
    [TableList(ShowIndexLabels = true)]
    public List<BallData> BallDataList;


    List<Entity> gunEnties;

    /// <summary>
    /// 动画终点位置
    /// </summary>
    public List<Transform> TargetPositionList;
    public Transform BallSelectedContent;
    void Start()
    {
        //ui
        InitSelectedBallPanel();


        em = PlayerEcsConnect.Instance.EntityManager;
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

    #region ui 逻辑
    [SerializeField] PanelBallSelect panelBallSelect;
    [SerializeField] PanelFreeSelectLib panelFreeSelectLib;

    public void InitSelectedBallPanel()
    {
        //选择球界面提交到切换球面板
        panelFreeSelectLib.SubmitAction = (balls) => { panelBallSelect.SetSoltList(balls); panelBallSelect.SetBallChangeUI(); };

        //选球槽数量
        panelFreeSelectLib.MaxSelectNum = panelBallSelect.SoltMaxNum;
    }

    #endregion

    #region Func
    EntityManager em;
    // 通用类型所有球可用,专用类型指定球可用.

    public void AddDamage(Entity gunEntity, int val)
    {
        var item = gunEntity;

        var ball = em.GetComponentData<CharacterGun>(item).Bullet;
        var damage = em.GetComponentData<Damage>(ball);
        damage.DamageValue += val;
        em.SetComponentData(ball, damage);
    }

    public void AddAbAddDamageOnCatch(Entity gunEntity, int val)
    {
        var item = gunEntity;
        {
            var ball = em.GetComponentData<CharacterGun>(item).Bullet;
            em.AddComponentData(ball, new DamageAddOnCatchTag());
        }
    }

    //速度下限增加.
    public void AddSpeed(Entity gunEntity, int val)
    {
        var item = gunEntity;
        {
            var ball = em.GetComponentData<CharacterGun>(item).Bullet;
            var bullet = em.GetComponentData<BulletComponent>(ball);
            bullet.SpeedRange.x += val;
            em.SetComponentData(ball, bullet);
        }
    }

    public void AddSize(Entity gunEntity , int val)
    {
        var ball = em.GetComponentData<CharacterGun>(gunEntity).Bullet;
        var scale = em.GetComponentData<CompositeScale>(ball);
        scale.Value = scale.Value + float4x4.Scale(val / 10.0f); //大小 +0.1
        scale.Value.c3.w = 1;
        em.SetComponentData(ball, scale);


        var collider = em.GetComponentData<PhysicsCollider>(ball);
        unsafe
        {
            Unity.Physics.SphereCollider* bcPtr = (Unity.Physics.SphereCollider*)collider.ColliderPtr;
            var boxGeometry = bcPtr->Geometry;
            boxGeometry.Radius = scale.Value.c0.x / 2;//球体缩放只需要取一个;
            bcPtr->Geometry = boxGeometry;
        }
    }

    //球之间不再碰撞.
    public void Boson(int val)
    {
    }

    /// <summary>
    /// 执行能力到实体
    /// </summary>
    public void ApplyCardAb(BallAbillityMap card)
    {
        foreach (var item in gunEnties)
        {
            var gun = em.GetComponentData<CharacterGun>(item);
            if (gun.ID != card.CategoryId)
            { continue; }

            //添加到对应球
            card.CallFunc(item);
        }
    }

    #endregion


    #region UI
    /// <summary>
    /// 用ab map字段填充卡牌ui
    /// </summary>
    /// <param name=""></param>
    public void FillAbCard(ICardUI item , int cardId)
    {
        var abCard = AbillityList[cardId];
        //item.SetBackGroudColor(BgColor);

        //set buff card
        item.SetCard(abCard.cardRef.Name.GetLocalizedString(), abCard.cardRef.Description.GetLocalizedString(), abCard.cardRef.PreviewImage, 0);
        item.CardId = cardId;
    }

    public void FillBallCard(ICardUI item , int ballId)
    {
        var ballData = BallDataList[ballId];

        item.SetCard(ballData.BallUI.PreviewImage, ballData.Price);
    }

    #endregion
}

public interface ICardUI
{
    public void SetCard(string title, string detail, Sprite icon, int rank);
    public void SetCard(Sprite icon, int num);
    public void SetIntactionable(bool opt);
    public void SetSelected(bool opt);
    public int CardId { get; set; }

    public UnityAction SubmitAction { get; set; }
}
