using Unity.Burst;
using Unity.Entities;
using Unity.Mathematics;
using Unity.Transforms;

[UpdateBefore(typeof(TransformSystemGroup))]
partial class AnimationBlobSystem : SystemBase
{
    protected override void OnUpdate()
    {
        const float k_Two_Pi = 2f * math.PI;
        var t = (float)Time.ElapsedTime;
        var deltaTime = Time.DeltaTime;
        //AnimateJob job = new AnimateJob()
        //{
        //    deltaTime = UnityEngine.Time.deltaTime,
        //};
        //var handle = job.Schedule(Dependency);

        Entities.ForEach((ref BlobAnimationClip anim,  ref Translation p,ref Rotation r) =>
        {
            //var normalizedTime = 0.5f * (math.sin(k_Two_Pi * 1 * (t)) + 1f);
            //scale.Value = math.lerp(anim.localPosition, new float3(1, 1, 1), normalizedTime);

            ref AnimationBlobAsset blob = ref anim.animBlobRef.Value;

            anim.timer += deltaTime;
            if (anim.timer < blob.frameDelta)
                return;

            while (anim.timer > blob.frameDelta)
            {
                anim.timer -= blob.frameDelta;
                anim.frame = (anim.frame + 1) % blob.frameCount;
            }

            anim.localPosition = blob.positions[anim.frame];
            //scale.Value = blob.scales[anim.frame]; , ref NonUniformScale scale ,
            p.Value = blob.positions[anim.frame];
            r.Value = quaternion.Euler(blob.eulers[anim.frame]);
            

        }).Run();


        //Entities.ForEach((ref NonUniformScale scale, in AnimationComponent data) =>
        //{
        //    var normalizedTime = 0.5f * (math.sin(k_Two_Pi * 1 * (t )) + 1f);
        //    scale.Value = math.lerp(data.localPosition, new float3(1,1,1), normalizedTime);
        //}).ScheduleParallel();
    }
    // BUG 筛选不到comp， 系统不运行，, ref NonUniformScale scale , 找不到？
    [BurstCompile]
    partial struct AnimateJob : IJobEntity
    {
        public float deltaTime;

        public void Execute(ref BlobAnimationClip anim, ref NonUniformScale scale)
        {
            ref AnimationBlobAsset blob = ref anim.animBlobRef.Value;

            anim.timer += deltaTime;
            if (anim.timer < blob.frameDelta)
                return;

            while (anim.timer > blob.frameDelta)
            {
                anim.timer -= blob.frameDelta;
                anim.frame = (anim.frame + 1) % blob.frameCount;
            }

            anim.localPosition = blob.positions[anim.frame];
            scale.Value = blob.scales[anim.frame];
        }
    }
}
