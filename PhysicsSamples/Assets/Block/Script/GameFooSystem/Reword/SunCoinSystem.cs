using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

public partial class SunCoinSystem : SystemBase
{
    protected override void OnUpdate()
    {
        // Assign values to local variables captured in your job here, so that it has
        // everything it needs to do its work when it runs later.
        // For example,
        //     float deltaTime = Time.DeltaTime;

        // This declares a new kind of job, which is a unit of work to do.
        // The job is declared as an Entities.ForEach with the target components as parameters,
        // meaning it will process all entities in the world that have both
        // Translation and Rotation components. Change it to process the component
        // types you want.

        //阳光产出的掉落动画
        //碰到挡板时消失 ，增加阳光

        Entities.ForEach((in SunCoinComponent sunCoinComponent) => {
        }).Schedule();
    }
}
