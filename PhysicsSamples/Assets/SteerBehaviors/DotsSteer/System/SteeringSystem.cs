using Unity.Burst;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;
using Unity.Collections;
namespace Steer
{
    /// <summary>
    /// 让包含vehicle的实体根据steer 运动
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public abstract partial class SteeringSystem<T>: SystemBase where T : struct, IComponentData, ISteering
    {
        EntityQuery m_Query, m_SteerForPointQuery, m_SteerForPursuitQuery, m_SteerForEvasionMapQuery;
        List<VehicleSharedData> m_UniqueTypes = new List<VehicleSharedData>();
        protected override void OnCreate()
        {
            // 创建一个组件过滤器
            m_Query = GetEntityQuery(new EntityQueryDesc
            {
                All = new[] { ComponentType.ReadOnly<VehicleSharedData>() }
            });

            RequireForUpdate(m_Query);
        }

        protected override void OnUpdate()
        {
            List<VehicleSharedData> uniqueTypes = new List<VehicleSharedData>();
            EntityManager.GetAllUniqueSharedComponentData(uniqueTypes);

            for (int boidVariantIndex = 0; boidVariantIndex < uniqueTypes.Count; boidVariantIndex++)
            {
                var setting = uniqueTypes[boidVariantIndex];

                var steerForceJob = CalculateForce(setting);
                var sumforceJob = new SumForceJob();
                sumforceJob.steerTypeHandle = this.GetComponentTypeHandle<T>(false);
                sumforceJob.forceTypeHandle = this.GetComponentTypeHandle<SteerData>(false);
                sumforceJob.Schedule(m_Query, Dependency);
            }
        }

        protected abstract JobHandle CalculateForce(VehicleSharedData setting);


        /// <summary>
        ///         Adds one to every translation component
        /// </summary>
        protected partial struct SumForceJob : IJobEntityBatch
        {
            public ComponentTypeHandle<T> steerTypeHandle;
            public ComponentTypeHandle<SteerData> forceTypeHandle;
            public void Execute(ArchetypeChunk batchInChunk, int batchIndex)
            {
                NativeArray<T> steers = batchInChunk.GetNativeArray(steerTypeHandle);
                NativeArray<SteerData> forces = batchInChunk.GetNativeArray(forceTypeHandle);
                for (int i = 0; i < batchInChunk.Count; i++)
                {
                    var force = forces[i];
                    force.Force += steers[i].WeightForce;
                    forces[i] = force;
                }
            }
        }
        //moveData
        public static float3 GetSeekVector(float3 target, float3 postion, float arrivalRadius, float3 velocity, bool considerVelocity = false)
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

        //public static float3 GetForceFromMap(NativeHashMap<int , float3> map, int key)
        //{
        //    var force = float3.zero;
        //    map.TryGetValue(key, out force);
        //    return force;
        //}

        public static int IntervalComparison(float x, float lowerBound, float upperBound)
        {
            if (x < lowerBound) return -1;
            if (x > upperBound) return +1;
            return 0;
        }
    }
}
