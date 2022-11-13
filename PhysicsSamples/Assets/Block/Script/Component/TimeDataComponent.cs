using System.Collections;
using UnityEngine;

using Unity.Core;
using Unity.Entities;

public struct TimeDataComponent : IComponentData
{
    public TimeData Value;
}

partial class TimeSystem : SystemBase
{
    protected override void OnUpdate()
    {
        var deltaTime = Time.DeltaTime;
        //var elapsedTime = Time.ElapsedTime;

        Entities
            .ForEach((ref TimeDataComponent time) =>
        {
            time.Value = new TimeData(time.Value.ElapsedTime + deltaTime, deltaTime);
        }).Schedule();
    }
}
