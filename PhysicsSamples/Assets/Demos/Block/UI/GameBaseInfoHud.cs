using System.Collections;
using System.Collections.Generic;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;

public class GameBaseInfoHud : MonoBehaviour
{
    EntityManager entityManager;

    EntityQuery blockGroup;
    EntityQuery bulletGroup;
    EntityQuery gunGroup;


    [SerializeField] IntEventChannelSO blockNumEvent;//改变时触发
    [SerializeField] IntEventChannelSO bulletNumEvent;
    [SerializeField] Vector2IntEventChannelSO bulletCapacityInfoEvent;

    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blockGroup = entityManager.CreateEntityQuery(
            new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BrickComponent)
                }
            });

        bulletGroup = entityManager.CreateEntityQuery(
            new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BulletComponent)
                }
            });
        gunGroup = entityManager.CreateEntityQuery(
            new EntityQueryDesc
            {
                None = new ComponentType[]
                {
                    typeof(DisableTag)
                },
                All = new ComponentType[]
                {
                    typeof(CharacterGun)
                }
            });

        oldBoxNum = blockGroup.CalculateEntityCount();
        oldBallNum = bulletGroup.CalculateEntityCount();
    }

    int oldBoxNum = 0;
    int oldBallNum = 0;
    void Update()
    {
        var currentBoxNum = blockGroup.CalculateEntityCount();
        var currentBallNum = bulletGroup.CalculateEntityCount();//存在的球数量
        if (currentBoxNum != oldBoxNum)
        {
            blockNumEvent.RaiseEvent(currentBoxNum);
        }
        if (oldBallNum != currentBallNum)
        {
            bulletNumEvent.RaiseEvent(currentBallNum);
        }


        var guns = gunGroup.ToEntityArray(Allocator.TempJob);
        var length = guns.Length;
        for (int i = 0; i < length; i++)
        {
            var gun = entityManager.GetComponentData<CharacterGun>(guns[i]);
            var cap =   gun.Capacity;
            bulletCapacityInfoEvent.RaiseEvent(new Vector2Int(cap, gun.MaxCapcity));
        }
        guns.Dispose();
    }

    public void Add100Bullet()
    {
        var guns = gunGroup.ToEntityArray(Allocator.TempJob);
        var length = guns.Length;
        for (int i = 0; i < length; i++)
        {
            var gun = entityManager.GetComponentData<CharacterGun>(guns[i]);
            gun.Capacity += 100;
            entityManager.SetComponentData<CharacterGun>(guns[i], gun);
        }


        guns.Dispose();
    }
}
