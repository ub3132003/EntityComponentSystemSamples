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
/// 球能力管理器，杂项ui 管理器，经济系统玩家交互管理器
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
    [SerializeField] int sunCoinNum;
    [TabGroup("BallData")]
    [SerializeField] IntEventChannelSO sunCoinEvent;

    [TabGroup("BallData")]
    [TableList(ShowIndexLabels = true)]
    public List<BallData> BallDataList;

    //所有gun 提前放到场景种
    List<Entity> gunEnties => PlayerEcsConnect.Instance.GunEnties;
    public int SunCoinNum
    {
        get => sunCoinNum;
        set { sunCoinNum = value; sunCoinEvent?.RaiseEvent(SunCoinNum); }
    }

    EntityManager em => PlayerEcsConnect.Instance.EntityManager;
    /// <summary>
    /// 动画终点位置
    /// </summary>
    public List<Transform> TargetPositionList;
    public Transform BallSelectedContent;
    void Start()
    {
        //ui
        InitSelectedBallPanel();
    }

    #region 工具方法,游戏逻辑
    class GoodsItem
    {
        public ThingSO itemSO;//id?
        public float Price = 1;
        public int Amount = 1;
        public int StatckMax = 99;
    }
    [System.Serializable]
    class Store
    {
        List<GoodsItem> goods;

        //补充全部球的价格
        public int BuyAllBallPrice = 200;
        /// <summary>
        /// 交换第一个商品一次
        /// </summary>
        /// <returns></returns>
        public int Buyball(ref int coin)
        {
            var item = goods[0];
            return 0;
        }

        /// <summary>
        /// 购买num 个ball 类型的球
        /// </summary>
        /// <param name="ball"></param>
        /// <param name="num"></param>
        /// <returns>失败 0 成功 >0 </returns>
        ///
        public int BuyBall(ThingSO ball, int num, ref int coin)
        {
            return 0;
        }

        /// <summary>
        /// 购买第idx个位置的物品， 扣除计算扣除后的价格coin。
        /// </summary>
        /// <param name="goodsIdx"></param>
        /// <param name="num"></param>
        /// <param name="coin"></param>
        /// <returns>0成功 其他 错误代码</returns>
        public int BuyBall(int goodsIdx , int num , ref int coin , out ThingSO ballSO)
        {
            var item = goods[goodsIdx];
            ballSO = item.itemSO;
            return 0;
        }
    }

    [SerializeField]
    Store BallStore = new Store();

    public void BuyBall(ThingSO ballSO , int amount = 1)
    {
        int coin = SunCoinNum;
        switch (BallStore.BuyBall(ballSO, amount, ref coin))
        {
            case 0:
                //购买成功
                AddBall(FindGun(ballSO) , amount);
                break;
            default:
                Debug.Log("购买失败");
                return;
        }

        SunCoinNum = coin;
    }

    //增加gun中的容量
    public void AddBall(ThingSO ballSO, int amount)
    {
        AddBall(FindGun(ballSO), amount);
    }

    public void AddBall(Entity gunEntity , int num)
    {
        var gun = em.GetComponentData<CharacterGun>(gunEntity);
        gun.Capacity += num;
        em.SetComponentData<CharacterGun>(gunEntity, gun);
    }

    public void BuyAllBullet(int amount = 1)
    {
        if (SunCoinNum < BallStore.BuyAllBallPrice)
        {
            Debug.Log("买球金币不足");
            return;
        }
        SunCoinNum -= BallStore.BuyAllBallPrice;
        var length = gunEnties.Count;

        for (int i = 0; i < length; i++)
        {
            var gun = em.GetComponentData<CharacterGun>(gunEnties[i]);
            gun.Capacity += amount;
            em.SetComponentData<CharacterGun>(gunEnties[i], gun);
        }
    }

    /// <summary>
    /// 查找gun实体,通过instanceID
    /// </summary>
    private Entity FindGun(int instanceId)
    {
        Entity gunEnity = gunEnties.Find(x =>
            em.GetComponentData<CharacterGun>(x).ID == instanceId);
        //todo 没找到时处理
        return gunEnity;
    }

    private Entity FindGun(ThingSO ballSO)
    {
        var instanceId = ballSO.Prefab.GetInstanceID();
        return FindGun(instanceId);
    }

    /// <summary>
    /// 通过球的so 切换激活的球,并禁用其他的球
    /// </summary>
    /// <param name="ballSO"></param>
    /// <returns></returns>
    public int ChangeGun(ThingSO ballSO)
    {
        foreach (var item in gunEnties)
        {
            ActiveBallEntity(item, false);
        }
        ActiveBallEntity(FindGun(ballSO), true);
        return 0;
    }

    /// <summary>
    /// 当前激活的gun 状态为fire
    /// </summary>
    /// <param name="ballSO"></param>
    public void SetFireGun(ThingSO ballSO)
    {
        foreach (var item in gunEnties)
        {
            if (em.HasComponent<DisableTag>(item))
                continue;
            var gun = em.GetComponentData<CharacterGun>(item);
            gun.IsFiring = 1;
            em.SetComponentData(item, gun);
        }
    }

    /// <summary>
    /// 切换当前可用的球
    /// </summary>
    /// <param name="gun"></param>
    /// <param name="opt"></param>
    public void ActiveBallEntity(Entity gun, bool opt)
    {
        if (opt)
        {
            em.RemoveComponent<DisableTag>(gun);
            var id = em.GetComponentData<CharacterGun>(gun).ID;
            activeBallEvent.RaiseEvent(id);
        }
        else
        {
            PlayerEcsConnect.Instance.EntityManager.AddComponent<DisableTag>(gun);
        }
    }

    #endregion
    #region ui 逻辑

    [SerializeField] PanelBallSelect panelBallSelect;
    [SerializeField] PanelFreeSelectLib panelFreeSelectLib;

    public void InitSelectedBallPanel()
    {
        //选择球界面提交到切换球面板
        panelFreeSelectLib.SubmitAction = (balls) => {
            panelBallSelect.SetSoltList(balls);
            panelBallSelect.LoadGunParis();
            panelBallSelect.SetBallChangeUI();
        };

        //选球槽数量
        panelFreeSelectLib.MaxSelectNum = panelBallSelect.SoltMaxNum;
    }

    [SerializeField] IntEventChannelSO activeBallEvent;


    #endregion

    #region Func，球的额外能力

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

    #region debug 非正式功能
    /// <summary>
    /// 所有球数量加100
    /// </summary>
    public void Add100Bullet()
    {
        var length = gunEnties.Count;
        for (int i = 0; i < length; i++)
        {
            var gun = em.GetComponentData<CharacterGun>(gunEnties[i]);
            gun.Capacity += 100;
            em.SetComponentData<CharacterGun>(gunEnties[i], gun);
        }
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
