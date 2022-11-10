using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.Jobs;
using Unity.Mathematics;
using Unity.Transforms;

namespace Rival.Samples.OnlineFPS
{
    [UpdateInGroup(typeof(SimulationSystemGroup), OrderFirst = true)]
    public partial class WeaponAssignmentSystem : SystemBase
    {
        private EntityCommandBufferSystem ecbSys;
        protected override void OnCreate()
        {
            base.OnCreate();

            ecbSys = World.GetOrCreateSystem<EndSimulationEntityCommandBufferSystem>();
        }

        protected override void OnUpdate()
        {
            EntityCommandBuffer commandBuffer = ecbSys.CreateCommandBuffer();
            ComponentDataFromEntity<Parent> parentFromEntity = GetComponentDataFromEntity<Parent>(true);
            ComponentDataFromEntity<LocalToParent> localToParentFromEntity = GetComponentDataFromEntity<LocalToParent>(true);
            BufferFromEntity<LinkedEntityGroup> linkedEntityBufferFromEntity = GetBufferFromEntity<LinkedEntityGroup>(false);

            Entities
                .WithoutBurst()
                .WithStructuralChanges()
                .ForEach((Entity entity, ref ActiveWeapon activeWeapon) =>
                {
                    // Handle assigning new active weapon
                    if (activeWeapon.WeaponEntity != activeWeapon.PreviousWeaponEntity)
                    {
                        Weapon weapon = GetComponent<Weapon>(activeWeapon.WeaponEntity);
                        weapon.OwnerEntity = entity;
                        // For characters, make View our shoot raycast start point
                        if (HasComponent<TopDownCharacterComponent>(entity))
                        {
                            TopDownCharacterComponent character = GetComponent<TopDownCharacterComponent>(entity);
                            //weapon.ShootOriginOverride = character.ViewEntity;
                        }
                        SetComponent(activeWeapon.WeaponEntity, weapon);

                        if (HasComponent<TopDownCharacterComponent>(entity))
                        {
                            TopDownCharacterComponent onlineFPSCharacter = GetComponent<TopDownCharacterComponent>(entity);
                            OnlineFPSUtilities.SetParent(
                                commandBuffer,
                                parentFromEntity,
                                localToParentFromEntity,
                                onlineFPSCharacter.WeaponSocketEntity,
                                activeWeapon.WeaponEntity,
                                default,
                                quaternion.identity);

                            DynamicBuffer<LinkedEntityGroup> linkedEntityBuffer = linkedEntityBufferFromEntity[entity];
                            linkedEntityBuffer.Add(new LinkedEntityGroup { Value = activeWeapon.WeaponEntity });
                        }

                        activeWeapon.PreviousWeaponEntity = activeWeapon.WeaponEntity;
                    }
                }).Run();

            ecbSys.AddJobHandleForProducer(Dependency);
        }
    }
}
