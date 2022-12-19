using System.Collections.Generic;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics;
using Unity.Transforms;
using Unity.Jobs;
using Unity.Collections;
using Stree;
using Unity.Physics.Extensions;
using Unity.Burst;
//using UnityEngine;
namespace Steer
{
    public partial class VehicleSystem : SystemBase
    {
        EntityQuery m_Query, m_SteerQuery;

        protected override void OnCreate()
        {
            // 创建一个组件过滤器
            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<VehicleSharedData>() , typeof(VehicleData)},
                Any = new ComponentType[] { typeof(AutonomousVehicle), typeof(BipedMove) },
            });

            RequireForUpdate(m_Query);

            //m_SteerQuery = GetEntityQuery(new EntityQueryDesc
            //{
            //    Any = new ComponentType[] { typeof(SteerForPoint), typeof(SteerForPursuit) }
            //});
            //RequireForUpdate(m_SteerForPointQuery);
            //RequireForUpdate(m_SteerForPursuitQuery);
        }

        protected override void OnUpdate()
        {
            var deltaTime = Time.DeltaTime;
            List<VehicleSharedData> m_UniqueTypes = new List<VehicleSharedData>();
            EntityManager.GetAllUniqueSharedComponentData(m_UniqueTypes);
            //根据 不同种类的vehicle计算运动
            for (int boidVariantIndex = 0; boidVariantIndex < m_UniqueTypes.Count; boidVariantIndex++)
            {
                var setting = m_UniqueTypes[boidVariantIndex];
                m_Query.AddSharedComponentFilter(setting);
                var boidCount = m_Query.CalculateEntityCount();

                if (boidCount == 0)
                {
                    m_Query.ResetFilter();
                    continue;
                }

                var world = World.Unmanaged;
                var cellForce = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);

                var adjustedVelocityArray = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);

                var targetSpeedArray = CollectionHelper.CreateNativeArray<float, RewindableAllocator>(boidCount, ref world.UpdateAllocator);
                var orientationVelocityArray = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);

                var accelerationArray = CollectionHelper.CreateNativeArray<float3, RewindableAllocator>(boidCount, ref world.UpdateAllocator);
                //写入目标方向
                Entities
                    .ForEach((int entityInQueryIndex, Entity entity, ref VehicleData vehicleData, ref SteerData steerData) =>
                {
                    var CanMove = true;
                    float Mass = 1;
                    if (!CanMove || setting.MaxForce.Approximately(0) || setting.MaxSpeed.Approximately(0))
                    {
                        return;
                    }
                    //LastRawForce = force;

                    var force = steerData.Force;
                    steerData.Force = 0;
                    float3 newVelocity = MathExtension.ClampMagnitude(force / Mass, setting.MaxForce);

                    if (math.lengthsq(newVelocity) == 0)
                    {
                        vehicleData.TargetSpeed = 0;
                        vehicleData.DesiredVelocity = float3.zero;
                    }
                    else
                    {
                        vehicleData.DesiredVelocity = newVelocity;
                    }

                    var adjustedVelocity = steerData.passForce;
                    steerData.passForce = 0;

                    if (!adjustedVelocity.Equals(float3.zero))
                    {
                        adjustedVelocity = MathExtension.ClampMagnitude(adjustedVelocity, setting.MaxSpeed);

                        newVelocity = adjustedVelocity;
                    }
                    vehicleData.NewVelocity = newVelocity;
                }).Schedule();
                Dependency.Complete();

                Entities
                    .WithName("ApplySteeringForce")
                    .WithSharedComponentFilter(setting)
                    .ForEach((ref Translation translation, in VehicleData vehicle) =>
                    {
                        var acceleration = vehicle.Acceleration;
                        acceleration = acceleration * setting.AllowedMovementAxes;
                        translation.Value += acceleration;
                        //TODO 刚体
                    }).Schedule();
                Entities
                    .WithName("AdjustOrientation")
                    .WithSharedComponentFilter(setting)
                    .ForEach((ref Rotation rotation, in LocalToWorld ltw, in VehicleData vehicle) =>
                    {
                        if (vehicle.TargetSpeed > setting.MinSpeedForTurning && !vehicle.Velocity.Equals(float3.zero))
                        {
                            var forward = ltw.Forward;
                            var newForward = math.normalize(vehicle.OrientationVelocity * setting.AllowedMovementAxes);
                            if (setting.TurnTime > 0)
                            {
                                newForward = UnityEngine.Vector3.Slerp(forward, newForward, deltaTime / setting.TurnTime);
                            }
                            rotation.Value = quaternion.LookRotationSafe(newForward, math.up());
                        }
                    }).Schedule();

                //Entities
                //    .WithSharedComponentFilter(setting)
                //    .ForEach((int entityInQueryIndex, in SteerForPoint steer) =>
                //    {
                //        cellForce[entityInQueryIndex] += steer.WeighedForce;
                //    }).Schedule(Dependency);

                //Entities
                //    .WithSharedComponentFilter(setting)
                //    .ForEach((int entityInQueryIndex, in SteerForPursuit steer) =>
                //    {
                //        cellForce[entityInQueryIndex] += steer.WeighedForce;
                //    }).Schedule(Dependency);

                //Entities
                //    .WithSharedComponentFilter(setting)
                //    .WithName("SetCalculatedVelocity_Autonomous")
                //    .ForEach((int entityInQueryIndex, ref AutonomousVehicle vehicle, in PhysicsVelocity pv, in PhysicsMass pm) =>
                //    {
                //        if (vehicle.CanMove)
                //        {
                //            var force = cellForce[entityInQueryIndex];
                //            float3 newVelocity = ClampMagnitude(force * vehicle.InverseMass, setting._maxSpeed);
                //            float TargetSpeed;
                //            float3 DesiredVelocity;
                //            if (math.lengthsq(newVelocity) == 0)
                //            {
                //                TargetSpeed = 0;
                //                DesiredVelocity = float3.zero;
                //            }
                //            else
                //            {
                //                vehicle.DesiredVelocity = newVelocity;
                //            }
                //            newVelocityArray[entityInQueryIndex] = newVelocity;
                //        }
                //    }).Schedule(Dependency);


                // pass force 后处理力
                //Entities
                //    .WithSharedComponentFilter(setting)
                //    .ForEach((int entityInQueryIndex, in SteerForEvasion steer) =>
                //    {
                //        cellForce[entityInQueryIndex] += steer.WeighedForce;
                //    }).Schedule(Dependency);

                //Entities
                //    .WithSharedComponentFilter(setting)
                //    .ForEach((int entityInQueryIndex, in SteerForce steerForce) =>
                //    {
                //        newVelocityArray[entityInQueryIndex] = steerForce.PassForce;
                //    }).Schedule(Dependency);
            }
        }
    }
}
