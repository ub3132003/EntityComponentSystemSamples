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

    //listening in
    [SerializeField] IntEventChannelSO activeBallEvent;
    public void OnEnable()
    {
        activeBallEvent.OnEventRaised += ChangeBallView;
    }

    public void OnDisable()
    {
        activeBallEvent.OnEventRaised -= ChangeBallView;
    }

    Transform currentView;
    void ChangeBallView(int ballId)
    {
        if (currentView != null)
        {
            Destroy(currentView.gameObject);
        }
        var ballPerfab = BallAbillityManager.Instance.BallDataList.Find(x => x.BallUI.Prefab.GetInstanceID() == ballId).BallUI.Prefab;

        currentView = Instantiate(ballPerfab.transform.GetChild(0) , transform);//外观
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
