using Unity.Mathematics;
using Unity.Entities;
namespace Steer
{
    /// <summary>
    /// Base class for vehicles. It does not move the objects, and instead
    /// provides a set of basic functionality for its subclasses.  See
    /// AutonomousVehicle for one that does apply the steering forces.
    /// </summary>
    /// <remarks>The main reasoning behind having a base vehicle class that is not
    /// autonomous in a library geared towards autonomous vehicles, is that in
    /// some circumstances we want to treat agents such as the player (which is not
    /// controlled by our automated steering functions) the same as other
    /// vehicles, at least for purposes of estimation, avoidance, pursuit, etc.
    /// In this case, the base Vehicle class can be used to provide an interface
    /// to whatever is doing the moving, like a CharacterMotor.</remarks>
    public struct VehicleSharedData : ISharedComponentData
    {
        public float _minSpeedForTurning { get; set; }//= 0.1f;

        /// <summary>
        /// The vehicle movement priority.
        /// </summary>
        /// <remarks>Used only by some behaviors to determine if a vehicle should
        /// be given priority before another one. You may disregard if you aren't
        /// using any behavior like that.</remarks>
        public int MovementPriority { get; set; }

        #region Private fields
        /// <summary>
        /// Across how many seconds is the vehicle's forward orientation smoothed
        /// </summary>
        /// <remarks>
        /// ForwardSmoothing would be a better name, but changing it now would mean
        /// anyone with a vehicle prefab would lose their current settings.
        /// </remarks>
        public float _turnTime { get; set; }//= 0.25f;

        /// <summary>
        /// Vehicle's mass
        /// </summary>
        /// <remarks>
        /// The total force from the steering behaviors will be divided by the
        /// vehicle mass before applying.
        /// </remarks>
        public float _mass { get; set; }// = 1;

        /// <summary>
        /// Indicates which axes a vehicle is allowed to move on
        /// </summary>
        /// <remarks>
        /// A 0 on the X/Y/Z value means the vehicle is not allowed to move on that
        /// axis, a 1 indicates it can.  We use Vector3Toggle to set it on the
        /// editor as a helper.
        /// </remarks>
        public float3 _allowedMovementAxes { get; set; }//= Vector3.one;

        /// <summary>
        /// The vehicle's arrival radius.
        /// </summary>
        /// <remarks>The difference between the radius and arrival radius is that
        /// the first is used to determine the area the vehicle covers, whereas the
        /// second one is a value used to determine if a vehicle is close enough
        /// to a desired target.  Unlike the radius, it is not scaled with the vehicle.</remarks>
        public float _arrivalRadius { get; set; }// = 0.25f;


        public float _maxSpeed { get; set; }//= 1;

        public float _maxForce { get; set; }//= 10;

        #endregion


        /// <summary>
        /// Minimum speed necessary for ths vehicle to apply a turn
        /// </summary>
        public float MinSpeedForTurning;

        /// <summary>
        /// Indicates which axes a vehicle is allowed to move on
        /// </summary>
        /// <remarks>
        /// A 0 on the X/Y/Z value means the vehicle is not allowed to move on that
        /// axis, a 1 indicates it can.  We use Vector3Toggle to set it on the
        /// editor as a helper.
        /// </remarks>
        public float3 AllowedMovementAxes;

        /// <summary>
        /// The velocity desired by this vehicle, likely calculated by means
        /// similar to what AutonomousVehicle does
        /// </summary>
        public float3 DesiredVelocity { get; set; }

        /// <summary>
        /// Maximum force that can be applied to the vehicle.  The sum of weighed
        /// steering forces will have its magnitude clamped to this value.
        /// </summary>
        public float MaxForce
        {
            get { return _maxForce; }
            set { _maxForce = math.clamp(value, 0, float.MaxValue); }
        }

        /// <summary>
        /// The vehicle's maximum speed
        /// </summary>
        public float MaxSpeed
        {
            get { return _maxSpeed; }
            set { _maxSpeed = math.clamp(value, 0, float.MaxValue); }
        }

        /// <summary>
        /// Radar assigned to this vehicle
        /// </summary>
        //public Radar Radar { get; private set; }


        /// <summary>
        /// Speedometer attached to the same object as this vehicle, if any
        /// </summary>
        //public Speedometer Speedometer { get; protected set; }


        /// <summary>
        /// Vehicle arrival radius
        /// </summary>
        public float ArrivalRadius
        {
            get { return _arrivalRadius; }
            set
            {
                _arrivalRadius = math.clamp(value, 0.01f, float.MaxValue);
                SquaredArrivalRadius = _arrivalRadius * _arrivalRadius;
            }
        }

        /// <summary>
        /// Squared arrival radius, for performance purposes
        /// </summary>
        public float SquaredArrivalRadius { get; set; }

        public float TurnTime
        {
            get { return _turnTime; }
            set { _turnTime = math.max(0, value); }
        }


        public float MaxFroce() { return 0; }
    }
    public interface IVehicle
    {
        /// <summary>
        /// The velocity desired by this vehicle, likely calculated by means
        /// similar to what AutonomousVehicle does
        /// </summary>
        public float3 DesiredVelocity { get; set; }

        /// <summary>
        /// Current vehicle speed
        /// </summary>
        public  float Speed { get; set; }

        /// <summary>
        /// Current vehicle velocity. Subclasses are likely to only actually
        /// implement one of the two methods.
        /// </summary>
        public  float3 Velocity { get; set; }

        /// <summary>
        /// Current magnitude for the vehicle's velocity.
        /// </summary>
        /// <remarks>
        /// It is expected to be set at the same time that the Velocity is
        /// assigned in one of the descendent classes.  It may or may not
        /// match the vehicle speed, depending on how that is calculated -
        /// for example, some subclasses can use a Speedometer to calculate
        /// their speed.
        /// </remarks>
        public float TargetSpeed { get; set; }

        /// <summary>
        /// Indicates if the current vehicle can move
        /// </summary>
        public bool CanMove { get; set; }
    }
}
