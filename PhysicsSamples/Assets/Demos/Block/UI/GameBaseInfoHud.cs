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


    [SerializeField] IntEventChannelSO blockNumEvent;
    [SerializeField] IntEventChannelSO bulletNumEvent;
    [SerializeField] IntEventChannelSO bulletNumNoShootEvent;
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
    }

    // Update is called once per frame
    void Update()
    {
        blockNumEvent.RaiseEvent(blockGroup.CalculateEntityCount());
        bulletNumEvent.RaiseEvent(bulletGroup.CalculateEntityCount());

        var guns = gunGroup.ToEntityArray(Allocator.TempJob);
        var length = guns.Length;
        for (int i = 0; i < length; i++)
        {
            var cap = entityManager.GetComponentData<CharacterGun>(guns[i]).Capacity;
            bulletNumNoShootEvent.RaiseEvent(cap);
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
