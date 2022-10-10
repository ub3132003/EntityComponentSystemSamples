using System;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;


[Serializable]
public struct BrickComponent : IComponentData
{
    //倒数命中计数，0时爆碎
    //public int HitCountDown;

    /// <summary>
    /// 死亡时掉落金币数量
    /// </summary>
    public int DieDropCount;
}

public class BrickComponentAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public int HitCountDown;

    public int DieDropCount;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BrickComponent
        {
            DieDropCount = DieDropCount,
        });
        dstManager.AddComponentData(entity, new FallDownComponent());

        dstManager.AddComponentData(entity, new Health
        {
            Value = HitCountDown
        });
    }
}
