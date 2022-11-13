using System.Collections;
using UnityEngine;
using Unity.Transforms;
namespace Unity.Mathematics
{
    public static class MathExtension
    {
        public static bool IsZero(this float4 num)
        {
            return num.x == 0 && num.y == 0 && num.z == 0 && num.w == 0 ? true : false;
        }

        public static bool IsZero(this float3 num)
        {
            return num.x == 0 && num.y == 0 && num.z == 0 ? true : false;
        }

        /// <summary>
        /// 返回旋转的 z轴方向
        /// </summary>
        /// <param name="rotation"></param>
        /// <returns></returns>
        public static float3 IsZero(this Rotation rotation)
        {
            return math.normalize(math.mul(rotation.Value, math.forward()));
        }

        public static bool IsTure(this bool2 num)
        {
            return num.x == true && num.y == true  ? true : false;
        }

        public static bool IsTure(this bool3 num)
        {
            return num.x == true && num.y == true && num.z == true ? true : false;
        }

        public static bool IsTure(this bool4 num)
        {
            return num.x == true && num.y == true && num.z == true && num.w == true ? true : false;
        }

        /// <summary>
        /// bool 转 +1 -1;
        /// </summary>
        /// <returns></returns>
        public static int ToDiff(this bool opt)
        {
            return opt ? 1 : -1;
        }
    }
}
