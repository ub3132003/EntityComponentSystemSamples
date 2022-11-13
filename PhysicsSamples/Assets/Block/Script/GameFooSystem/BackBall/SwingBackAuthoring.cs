using System;
using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
/// <summary>
/// 玩家点击大打回球,武器系統
/// </summary>

public struct SwingBack : IComponentData
{
    public Entity OwnEntity;
    /// <summary>
    /// 可回击冷却时间
    /// </summary>
    public float Rate;
}


public class SwingBackAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public GameObject OwnEntity;
    public float Rate;
    public ParticleSystem viwePs;

    private void Awake()
    {
        var psParam = viwePs.main;
        psParam.duration = Rate;
    }

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new SwingBack
        {
            OwnEntity = conversionSystem.GetPrimaryEntity(OwnEntity),
            Rate = Rate,
        });
    }
}
