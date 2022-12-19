using Unity.Mathematics;
using UnityEditor;
using UnityEngine;

namespace Steer
{
    [Sirenix.OdinInspector.InlineEditor]
    public class VehicleAgentSO : ScriptableObject
    {
        public float AccelerationRate = 5;

        public float DecelerationRate = 8;

        public float _minSpeedForTurning = 0.1f;


        public int MovementPriority;


        public float TurnTime = 0.25f;


        public float _mass = 1;


        public float3 AllowedMovementAxes = Vector3.one;


        public float ArrivalRadius = 0.25f;


        public float MaxSpeed = 1;

        public float MaxForce = 10;


        public float MinSpeedForTurning;

        public enum VehicleType
        {
            /// <summary>
            /// 鱼,汽车的移动方式
            /// </summary>
            AutonomousVehicle,

            /// <summary>
            /// 全向移动,类似蜘蛛坦克.
            /// </summary>
            Biped
        }
        public VehicleType vehicleType;
        public VehicleSharedData ToComponent()
        {
            return
                new VehicleSharedData
                {
                    _accelerationRate = AccelerationRate,
                    _decelerationRate = DecelerationRate,
                    _minSpeedForTurning = _minSpeedForTurning,
                    MovementPriority = MovementPriority,
                    TurnTime = TurnTime,
                    _mass = _mass,
                    AllowedMovementAxes = AllowedMovementAxes,
                    ArrivalRadius = ArrivalRadius,
                    MaxForce = MaxForce,
                    MaxSpeed = MaxSpeed,
                    MinSpeedForTurning = MinSpeedForTurning,
                };
        }
    }
}
