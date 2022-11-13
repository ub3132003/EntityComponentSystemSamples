using Unity.Entities;
using Unity.Mathematics;

namespace Unity.Rendering
{
    [MaterialProperty("_Emission", MaterialPropertyFormat.Float4)]
    struct EmissionVector4Override : IComponentData
    {
        public float4 Value;
    }
}
