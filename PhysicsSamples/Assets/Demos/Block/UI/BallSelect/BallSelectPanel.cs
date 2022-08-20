using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;
using Sirenix.OdinInspector;
using System.Linq;

namespace Assets.Demos.Block.UI.BallSelect
{    /// <summary>
    ///   gun 被玩家实体持有,所有潜在的gun 在场景中转为实体,,这个类,通过disableTag 激活.
    ///   启动时默认1号显示激活的gun
    /// </summary>
    public class BallSelectPanel : MonoBehaviour
    {
        [SerializeField] List<UIBallToggle> ballToggles;

        [ShowInInspector] List<Entity> gunEnties;
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
            var queryBuilder = new EntityQueryDescBuilder(Unity.Collections.Allocator.TempJob);
            queryBuilder.AddAll(typeof(CharacterGun));
            queryBuilder.FinalizeQuery();

            EntityQuery gunGroup = gunSystem.GetEntityQuery(queryBuilder);

            var guns = gunGroup.ToEntityArray(Unity.Collections.Allocator.TempJob);
            gunEnties = new List<Entity>(guns);

            queryBuilder.Dispose();
            guns.Dispose();

            //对应asset 和 实体map
            LoadGunParis();

            //初始化选项UI

            ballToggles = new List<UIBallToggle>(GetComponentsInChildren<UIBallToggle>());

            for (int i = 0; i < ballToggles.Count; i++)
            {
                var item = ballToggles[i];
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

        [System.Serializable]
        class SoltPari
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

        public void LoadGunParis()
        {
            foreach (var item in soltParis)
            {
                Entity gunEnity = gunEnties.Find(x =>
                    PlayerEcsConnect.Instance.EntityManager.GetComponentData<CharacterGun>(x).ID == item.ID);
                if (gunEnity == Entity.Null) continue;


                item.GunEntity = gunEnity;
                if (item.IsActive)
                {
                    PlayerEcsConnect.Instance.EntityManager.RemoveComponent<DisableTag>(gunEnity);
                }
                else
                {
                    PlayerEcsConnect.Instance.EntityManager.AddComponent<DisableTag>(gunEnity);
                }
            }
        }

        /// <summary>
        /// 更换选项
        /// </summary>
        /// <param name="soltId"></param>
        /// <param name="entity"></param>
        public void SetGunSolt(int soltId, Entity entity , bool isActive)
        {
            var idx = soltParis.FindIndex(x => x.SoltId == soltId);
            soltParis[idx] = new SoltPari { SoltId = soltId, GunEntity = entity , IsActive = isActive };
        }

        /// <summary>
        /// 激活gun 实体
        /// </summary>
        /// <param name="opt"></param>
        /// <param name="option"></param>
        void ActiveBall(bool opt , int option)
        {
            var gunSolt = soltParis.Find(x => x.SoltId == option);
            Entity gun = gunSolt.GunEntity;
            if (gun == Entity.Null) return;
            gunSolt.IsActive = opt;
            if (opt)
            {
                PlayerEcsConnect.Instance.EntityManager.RemoveComponent<DisableTag>(gun);
            }
            else
            {
                PlayerEcsConnect.Instance.EntityManager.AddComponent<DisableTag>(gun);
            }
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
}
