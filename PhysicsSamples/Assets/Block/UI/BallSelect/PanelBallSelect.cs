using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;
using Unity.Assertions;


/// <summary>
///   gun 被玩家实体持有,所有潜在的gun 在场景中转为实体,,这个类,通过disableTag 激活.
///   启动时默认1号显示激活的gun
/// </summary>
public class PanelBallSelect : MonoBehaviour
{
    [SerializeField] List<UIBallToggle> ballToggles;

    [ShowInInspector] List<Entity> gunEnties;
    IEnumerator Start()
    {
        while (PlayerEcsConnect.Instance.GunEnties == null || PlayerEcsConnect.Instance.GunEnties.Count <= 0)
        {
            yield return 0;
        }
        gunEnties = PlayerEcsConnect.Instance.GunEnties;
        //SetBallChangeUI();

        //激活默认的球
        //对应asset 和 实体map
        LoadGunParis();
    }

    [System.Serializable]
    public class SoltPari
    {
        public int SoltId;

        public ThingSO BulletUI;

        public GameObject Bullet => BulletUI.Prefab;
        public int ID => Bullet?.GetInstanceID() ?? 0;
        [ShowInInspector]
        public Entity GunEntity = Entity.Null;
        public bool IsActive;
    }
    [SerializeField] List<SoltPari> soltParis;

    public int SoltMaxNum => soltParis.Count;
    public void SetGunEnties(List<Entity> guns)
    {
        gunEnties = guns;
    }

    /// <summary>
    /// 设置切球面板
    /// </summary>
    public void SetBallChangeUI()
    {
        //初始化选项UI

        ballToggles = new List<UIBallToggle>(GetComponentsInChildren<UIBallToggle>());

        for (int i = 0; i < ballToggles.Count; i++)
        {
            var item = ballToggles[i];
            //设置选项序号
            item.Init(i);
            soltParis[i].SoltId = i;
            item.SetToggleEvent(ActiveBall);
            //设置ui 图标
            if (soltParis[i].GunEntity == Entity.Null)
            {
                item.SetBallUI(null);
            }
            else
            {
                item.SetBallUI(soltParis[i].BulletUI.PreviewImage);
            }
        }
    }

    /// <summary>
    /// 把球的信息填到预设的空槽上
    /// </summary>
    /// <param name="bullet"></param>
    public void SetSoltList(List<ThingSO> bullet)
    {
        Assert.Equals(bullet.Count, soltParis.Count);
        for (int i = 0; i < soltParis.Count; i++)
        {
            soltParis[i].BulletUI = bullet[i];
        }
    }

    /// <summary>
    /// 增加一个空球槽
    /// </summary>
    public void AddOneSolt()
    {
        var t = new SoltPari();
        t.SoltId = soltParis.Count + 1;
        soltParis.Add(t);
    }

    /// <summary>
    /// 更新那些球被激活，提交慢面板
    /// </summary>
    public void LoadGunParis()
    {
        foreach (var item in soltParis)
        {
            Entity gunEnity = gunEnties.Find(x =>
                PlayerEcsConnect.Instance.EntityManager.GetComponentData<CharacterGun>(x).ID == item.ID);
            if (gunEnity == Entity.Null) continue;


            item.GunEntity = gunEnity;
            BallAbillityManager.Instance.ActiveBallEntity(gunEnity, item.IsActive);
        }
    }

    /// <summary>
    /// 配置槽对应的gun
    /// </summary>
    /// <param name="soltId"></param>
    /// <param name="entity"></param>
    public void SetGunSolt(int soltId, Entity entity , bool isActive)
    {
        var idx = soltParis.FindIndex(x => x.SoltId == soltId);
        soltParis[idx] = new SoltPari { SoltId = soltId, GunEntity = entity , IsActive = isActive };
    }

    /// <summary>
    /// 激活gun 实体 ui,通过槽序号找到gun
    /// </summary>
    /// <param name="opt"></param>
    /// <param name="option">序号</param>
    void ActiveBall(bool opt , int option)
    {
        var gunSolt = soltParis.Find(x => x.SoltId == option);
        Entity gun = gunSolt.GunEntity;
        if (gun == Entity.Null) return;
        gunSolt.IsActive = opt;
        BallAbillityManager.Instance.ActiveBallEntity(gun, opt);
    }

    private void Update()
    {
        bool Alpha4 = Input.GetKeyDown(KeyCode.Alpha4);

        bool Alpha1 = Input.GetKeyDown(KeyCode.Alpha1);
        bool Alpha2 = Input.GetKeyDown(KeyCode.Alpha2);
        bool Alpha3 = Input.GetKeyDown(KeyCode.Alpha3);

        if (Alpha4)
        {
            ballToggles[3].SetToggleOn();
        }
        if (Alpha1)
        {
            ballToggles[0].SetToggleOn();
        }
        if (Alpha2)
        {
            ballToggles[1].SetToggleOn();
        }
        if (Alpha3)
        {
            ballToggles[2].SetToggleOn();
        }
    }
}