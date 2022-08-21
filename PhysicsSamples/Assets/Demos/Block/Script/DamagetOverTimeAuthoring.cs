using System.Collections;
using Unity.Entities;
using UnityEngine;


public struct DamagetOverTime : IComponentData, IDamage
{
    public int Value { get; set; }
    public COST_TYPES Type { get; set; }
}


public class DamagetOverTimeAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public int Value;
    public COST_TYPES Type;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new DamagetOverTime
        {
            Value = Value,
            Type = Type
        });
        dstManager.AddComponentData(entity, new TimeDataComponent());

        var buff = dstManager.AddBuffer<BuffEffectComponent>(entity);
    }
}
