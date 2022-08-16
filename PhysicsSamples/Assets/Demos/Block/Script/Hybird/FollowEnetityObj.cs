using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Stateful;
using Unity.Transforms;
using UnityEngine;

public class FollowEnetityObj : MonoBehaviour, IReceiveEntity
{
    private Entity m_DisplayEntity;
    public float3 Offset;
    public void SetReceivedEntity(Entity entity)
    {
        //bug 除了CharacterControllerComponentData能被找到其他组件找不到
        //if (World.DefaultGameObjectInjectionWorld.EntityManager.HasComponent<CharacterControllerComponentData>(entity))
        //{

        //}
        m_DisplayEntity = entity;
    }

    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        if (!World.DefaultGameObjectInjectionWorld.IsCreated ||
            !World.DefaultGameObjectInjectionWorld.EntityManager.Exists(m_DisplayEntity))
        {
            return;
        }


        var k_TextOffset = float3.zero;
        var pos = World.DefaultGameObjectInjectionWorld.EntityManager.GetComponentData<Translation>(m_DisplayEntity);
        transform.position = pos.Value + Offset;
    }
}
