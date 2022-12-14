using Unity.Entities;
using Unity.Mathematics;
namespace Steer
{
    /// <summary>
    /// Parent class for objects that vehicles can aim for, be it other vehicles or
    /// static objects.
    /// </summary>

    public struct DetectableObject : IComponentData
    {
        /// <summary>
        /// The vehicle's radius.
        /// </summary>
        private float _radius;//= 1;


        /// <summary>
        /// Collider attached to this object. The GameObject that the DetectableObject
        /// is attached to is expected to have at most one collider.
        /// </summary>
        //public Collider Collider { get; private set; }


        /// <summary>
        /// Vehicle center on the transform
        /// </summary>
        /// <remarks>
        /// This property's setter recalculates a temporary value, so it's
        /// advised you don't re-scale the vehicle's transform after it has been set
        /// </remarks>
        public float3 Center;

        /// <summary>
        /// Vehicle radius
        /// </summary>
        /// <remarks>
        /// This property's setter recalculates a temporary value, so it's
        /// advised you don't re-scale the vehicle's transform after it has been set
        /// </remarks>
        public float Radius
        {
            get { return _radius; }
            set
            {
                _radius = math.clamp(value, 0.01f, float.MaxValue);
                SquaredRadius = _radius * _radius;
            }
        }

        /// <summary>
        /// Calculated squared object radius
        /// </summary>
        public float SquaredRadius { get; private set; }


        #region Methods


        /// <summary>
        /// Recalculates the object's radius based on the transform's scale,
        /// using the largest of x/y/z as the scale value and multiplying it
        /// by a base.
        /// </summary>
        /// <param name="baseRadius">Base radius the object would have if the scale was 1</param>
        public void ScaleRadiusWithTransform(float baseRadius)
        {
            //var scale = Transform.lossyScale;
            //_radius = baseRadius * Mathf.Max(scale.x, Mathf.Max(scale.y, scale.z));
        }

        #endregion
    }
}
