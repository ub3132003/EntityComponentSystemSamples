using System.Collections;
using System.Collections.Generic;
using Unity.Burst;
using Unity.Entities;
using UnityEngine;

public struct HealthEvent : IBufferElementData
{
    public COMPARABLE_TYPE opt;
    public int Threshold;
    //触发状态
    public bool level;
    public TRIGGER state;
    public static HealthEvent CompareEvent(HealthEvent healthEvent, int value)
    {
        bool res = false;
        var opt = healthEvent.opt;
        var Threshold = healthEvent.Threshold;
        var level = healthEvent.level;
        var state = healthEvent.state;
        switch (opt)
        {
            case COMPARABLE_TYPE.GREAT:
                res = value > Threshold;

                break;
            case COMPARABLE_TYPE.EQUAL:
                res = value == Threshold;
                break;
            case COMPARABLE_TYPE.LESS:
                res = value < Threshold;
                break;
            default:
                break;
        }
        if (level == true && res == true)
        {
            state = TRIGGER.HIGH_LEVEL;
        }
        else if (level == true && res == false)
        {
            state = TRIGGER.FALLING_EDGE;
        }
        else if (level == false && res == true)
        {
            state = TRIGGER.RISING_EDGE;
        }
        else//level f , res f
        {
            state = TRIGGER.LOW_LEVEL;
        }
        level = res;

        return new HealthEvent
        {
            level = level,
            opt = opt,
            Threshold = Threshold,
            state = state,
        };
    }

    //public static class StatefulEventCollectionJobs
    //{
    //    // NOTE: ITrigger|CollisionEventsJob[Base] needs be used rather than the
    //    // non-Base version if this code is part of the Unity Physics package.

    //    [BurstCompile]
    //    public struct CollectTriggerEvents : IJobEntity
    //    {
    //        public NativeList<HealthEvent> TriggerEvents;
    //        public void Execute(TriggerEvent triggerEvent) => TriggerEvents.Add(new StatefulTriggerEvent(triggerEvent));
    //    }
    //}
}

public class HealthEventBufferAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [System.Serializable] class HealthEventData
    {
        public COMPARABLE_TYPE opt;
        public int Threshold;
    }
    [SerializeField] List<HealthEventData> healthEvents;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var buffer = dstManager.AddBuffer<HealthEvent>(entity);
        for (int i = 0; i < healthEvents.Count; i++)
        {
            var data = healthEvents[i];
            buffer.Add(new HealthEvent
            {
                opt = data.opt,
                Threshold = data.Threshold,
            });
        }
    }
}
