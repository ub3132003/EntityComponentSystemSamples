using Sirenix.OdinInspector;
using System.Collections.Generic;

using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using UnityEngine;
using UnityEngine.Events;

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
/// 球能力管理器
/// </summary>
public class BallAbillityManager : Singleton<BallAbillityManager>
{
    [AssetsOnly]
    public List<GameObject> BallPrefabs;


    [TableList(ShowIndexLabels = true)]
    [SerializeField]
    public List<BallAbillityMap> AbillityList;


    void Start()
    {
        em = PlayerEcsConnect.Instance.EntityManager;
    }

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

    #endregion
}
