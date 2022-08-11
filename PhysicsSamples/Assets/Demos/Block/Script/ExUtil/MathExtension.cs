using System.Collections;
using UnityEngine;

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
    }
}
