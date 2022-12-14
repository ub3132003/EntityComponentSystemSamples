using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Stree;
using Unity.Physics.Extensions;

namespace Steer
{
    partial class AutonomousVehicleMoveSystem : SystemBase
    {
        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            List<VehicleSharedData> uniqueTypes = new List<VehicleSharedData>();
            EntityManager.GetAllUniqueSharedComponentData(uniqueTypes);

            for (int boidVariantIndex = 0; boidVariantIndex < uniqueTypes.Count; boidVariantIndex++)
            {
                var setting = uniqueTypes[boidVariantIndex];

                Entities
                    .WithName("SetCalculatedVelocity")
                    .WithSharedComponentFilter(setting)
                    .WithAll<AutonomousVehicle>()
                    .ForEach((int entityInQueryIndex, ref Translation translation, ref VehicleData vehicle, in LocalToWorld localToWorld) =>
                    {
                        var speed = vehicle.Speed;
                        var newVelocity = vehicle.NewVelocity;
                        vehicle.TargetSpeed = math.length(newVelocity);
                        vehicle.OrientationVelocity = Approximately(speed, 0) ? localToWorld.Forward : newVelocity / vehicle.TargetSpeed;
                    }).Schedule(Dependency);

                Entities
                    .WithName("CalculatePositionDelta")
                    .WithSharedComponentFilter(setting)
                    .ForEach((ref Rotation rotation, ref VehicleData vehicle , in AutonomousVehicle autonomousVehicle, in LocalToWorld localToWorld) =>
                    {
                        var _speed = vehicle.Speed;
                        var targetSpeed = vehicle.TargetSpeed;
                        /*
                         * Notice that we clamp the target speed and not the speed itself,
                         * because the vehicle's maximum speed might just have been lowered
                         * and we don't want its actual speed to suddenly drop.
                         */
                        targetSpeed = math.clamp(targetSpeed, 0, setting.MaxSpeed);
                        if (Approximately(_speed, targetSpeed))
                        {
                            _speed = targetSpeed;
                        }
                        else
                        {
                            var rate = targetSpeed > _speed ? autonomousVehicle._accelerationRate : autonomousVehicle._decelerationRate;
                            _speed = math.lerp(_speed, targetSpeed, deltaTime * rate);
                        }


                        vehicle.Speed = _speed;
                        //Cannot set the velocity directly on AutonomousVehicle
                        vehicle.Velocity = localToWorld.Forward * _speed;
                        vehicle.Acceleration = localToWorld.Forward * _speed * deltaTime;
                    }).Schedule(Dependency);
            }
        }

        private void ApplySteeringForce(float elapsedTime)
        {
        }

        protected void AdjustOrientation(float deltaTime)
        {
        }

        protected void CalculateForces()
        {
        }

        static bool Approximately(float a, float b)
        {
            float delta = 0.01f;
            return math.abs(a - b) < delta;
        }

        //moveData
        static float3 GetSeekVector(float3 target, float3 postion, float arrivalRadius, float3 velocity, bool considerVelocity = false)
        {
            var force = float3.zero;
            var difference = target - postion;// translation.Value;
            var d = math.lengthsq(difference);
            if (d > arrivalRadius * arrivalRadius)
            {
                /* But suppose we still have some distance to go. The first step
                * then would be calculating the steering force necessary to orient
                * ourselves to and walk to that point.
                *
                * It doesn't apply the steering itself, simply returns the value so
                * we can continue operating on it.
                */
                force = considerVelocity ? difference - velocity : difference;
            }
            return force;
        }

        public static float3 PredictFuturePosition(float3 position, float3 velocity, float predictionTime)
        {
            return position + (velocity * predictionTime);
        }

        public static float3 PredictFutureDesiredPosition(float3 position, float3 desiredVelocity, float predictionTime)
        {
            return position + (desiredVelocity * predictionTime);
        }

        //public static float3 ClampMagnitude(float3 x, float max)
        //{
        //    if (math.lengthsq(x) > max * max)
        //    {
        //        return x / math.length(x) * max;
        //    }
        //    else
        //    {
        //        return x;
        //    }
        //}
    }
}
