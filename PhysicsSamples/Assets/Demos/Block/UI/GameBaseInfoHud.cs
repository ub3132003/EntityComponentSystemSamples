using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

public class GameBaseInfoHud : MonoBehaviour
{
    EntityQuery blockGroup;
    EntityQuery bulletGroup;
    EntityManager entityManager;

    [SerializeField] IntEventChannelSO blockNumEvent;
    [SerializeField] IntEventChannelSO bulletNumEvent;
    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        blockGroup = entityManager.CreateEntityQuery(
            new EntityQueryDesc
            {
                All = new ComponentType[]
                {
                    typeof(BlockComponent)
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
    }

    // Update is called once per frame
    void Update()
    {
        blockNumEvent.RaiseEvent(blockGroup.CalculateEntityCount());
        bulletNumEvent.RaiseEvent(bulletGroup.CalculateEntityCount());
    }
}
