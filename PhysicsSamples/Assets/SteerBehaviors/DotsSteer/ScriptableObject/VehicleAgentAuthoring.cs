using Stree;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
namespace Steer
{
    public partial class VehicleAgentAuthoring : MonoBehaviour
    {
        public VehicleAgentSO agentSO;
        public VehicleBehaviorSettingSO vehicleBehaviorSettingSO;
    }

    [DisallowMultipleComponent]
    public partial class VehicleAgentAuthoring : IConvertGameObjectToEntity
    {
        void IConvertGameObjectToEntity.Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
        {
            //BehaviorSettingsUpdateSystem will fill in the rest of the neccessary componentData when the change is detected

            dstManager.AddComponent<SteerData>(entity);
            dstManager.AddSharedComponentData(entity, agentSO.ToComponent());
            var steerSettings = vehicleBehaviorSettingSO.Behaviors;
            //读取so 添加steer
            for (int i = 0; i < steerSettings.Count; i++)
            {
                var steer = steerSettings[i];
                steer.AddComponentData(entity, dstManager);
            }
            switch (agentSO.vehicleType)
            {
                case VehicleAgentSO.VehicleType.AutonomousVehicle:
                    dstManager.AddComponentData(entity, new AutonomousVehicle());
                    break;
                case VehicleAgentSO.VehicleType.Biped:
                    dstManager.AddComponentData(entity, new BipedMove());
                    break;
                default:
                    break;
            }

            //AgentData holds everything a behavior needs to react to another Agent
            dstManager.AddComponentData(entity, new VehicleData());
            //dstManager.AddComponentData(entity, new AccelerationData { Value = float3.zero });
            //dstManager.AddComponentData(entity, new PerceptionData());

            //give entity a buffer to hold info about surroundings
            //dstManager.AddBuffer<NeighborData>(entity);
        }
    }
}
