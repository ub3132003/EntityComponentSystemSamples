using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Rival;
using Rival.Samples.OnlineFPS;
[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsShapeAuthoring))]
public class GameCharacterWeaponHolder : MonoBehaviour
{
    //public GameObject View;
    //public GameObject MeshRoot;
    public GameObject WeaponSocket;

    [UpdateAfter(typeof(EndColliderConversionSystem))]
    public class BasicKinematicCharacterConversionSystem : GameObjectConversionSystem
    {
        protected override void OnUpdate()
        {
            Entities.ForEach((GameCharacterWeaponHolder authoring) =>
            {
                Entity entity = GetPrimaryEntity(authoring.gameObject);

                //authoring.OnlineFPSCharacter.ViewEntity = GetPrimaryEntity(authoring.View);
                //authoring.OnlineFPSCharacter.MeshRootEntity = GetPrimaryEntity(authoring.MeshRoot);
                var activeWeapon = new ActiveWeapon();
                activeWeapon.WeaponSocketEntity = GetPrimaryEntity(authoring.WeaponSocket);

                //DstEntityManager.AddComponentData(entity, new OnlineFPSCharacterInputs());
                DstEntityManager.AddComponentData(entity, activeWeapon);
            });
        }
    }
}
