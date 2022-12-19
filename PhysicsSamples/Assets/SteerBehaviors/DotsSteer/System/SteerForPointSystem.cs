using Steer;
using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
[assembly: RegisterGenericJobType(typeof(SteeringSystem<SteerForPoint>.SumForceJob))]
namespace Steer
{
    public partial class SteerForPointSystem : SteeringSystem<SteerForPoint>
    {
        protected override JobHandle CalculateForce(VehicleSharedData setting)
        {
            return Entities
                .WithSharedComponentFilter(setting)
                .ForEach((ref SteerForPoint steerForPoint, in VehicleData vehicleData, in Translation translation) =>
                {
                    steerForPoint.WeightForce = steerForPoint.Weight * GetSeekVector(
                        steerForPoint.TargetPoint, translation.Value, setting.ArrivalRadius, vehicleData.Velocity, steerForPoint.ConsiderVelocity);
                }).ScheduleParallel(Dependency);
        }
    }
}
