using Unity.Collections;
using UnityEngine;
using UnityEngine.ParticleSystemJobs;
using System;
using System.Collections.Generic;
using Unity.Jobs;
using Unity.Burst;
/// <summary>
/// 跟随物体，到达后消失
/// </summary>
public class ParticleFollow : MonoBehaviour
{
    public float killRange = 2.0f;
    public float effectStrength = 1.0f; //bug 超过9的速度吸取 时到达位置不会消失
    public float FollowStartAliveTime = 10;

    public bool useJobSystem = false;


    private float oscillationPhase;

    private ParticleSystem ps;
    private UpdateParticlesJob job = new UpdateParticlesJob();
    private ParticleSystem.Particle[] mainThreadParticles;


    private static Vector3 CalculateVelocity(ref UpdateParticlesJob job, Vector3 delta)
    {
        float attraction = job.effectStrength;
        return delta.normalized * attraction;
    }

    void Start()
    {
        ps = GetComponent<ParticleSystem>();
    }

    public Transform target;
    void Update()
    {
        job.TargetPosition = target.position;
        job.effectRangeSqr = killRange * killRange;
        job.effectStrength = effectStrength;
        job.FollowStartAliveTime = FollowStartAliveTime;
    }

    NativeArray<Vector3> arr;


    JobHandle psJobHandle;
    void OnParticleUpdateJobScheduled()
    {
        if (useJobSystem)
        {
            psJobHandle = job.Schedule(ps);
        }
    }

    [BurstCompile]
    struct UpdateParticlesJob : IJobParticleSystem
    {
        [ReadOnly]
        public Vector3 TargetPosition;

        [ReadOnly]
        public float effectRangeSqr;

        [ReadOnly]
        public float effectStrength;

        [ReadOnly]
        public float inverseEffectRange;
        [ReadOnly]
        public float FollowStartAliveTime;
        public void Execute(ParticleSystemJobData particles)
        {
            var positionsX = particles.positions.x;
            var positionsY = particles.positions.y;
            var positionsZ = particles.positions.z;

            var velocitiesX = particles.velocities.x;
            var velocitiesY = particles.velocities.y;
            var velocitiesZ = particles.velocities.z;

            var aliveTime = particles.aliveTimePercent;

            for (int i = 0; i < particles.count; i++)
            {
                Vector3 position = new Vector3(positionsX[i], positionsY[i], positionsZ[i]);

                Vector3 delta = (TargetPosition - position);
                var sqrMagnitude = delta.sqrMagnitude;
                //过一会开始吸
                if (aliveTime[i] > FollowStartAliveTime)
                {
                    //进入范围消失
                    if (sqrMagnitude < 2 * 2)
                    {
                        aliveTime[i] = 100;
                    }
                    //吸取范围
                    else if (sqrMagnitude < effectRangeSqr)
                    {
                        Vector3 velocity = CalculateVelocity(ref this, delta);

                        velocitiesX[i] = velocity.x;
                        velocitiesY[i] = velocity.y;
                        velocitiesZ[i] = velocity.z;
                    }
                }
            }
        }
    }
}
