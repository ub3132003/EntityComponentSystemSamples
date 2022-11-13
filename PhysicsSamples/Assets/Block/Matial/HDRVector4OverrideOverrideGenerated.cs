using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("_HDR", MaterialPropertyFormat.Float4)]
    struct HDRVector4Override : IComponentData
    {
        public float4 Value;
    }
}
