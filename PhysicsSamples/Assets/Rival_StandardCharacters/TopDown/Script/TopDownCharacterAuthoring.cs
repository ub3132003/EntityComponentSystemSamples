using Unity.Entities;
using Unity.Mathematics;
using Unity.Physics.Authoring;
using UnityEngine;
using Rival;
using Unity.Physics;

[DisallowMultipleComponent]
[RequireComponent(typeof(PhysicsShapeAuthoring))]
[UpdateAfter(typeof(EndColliderConversionSystem))]
public class TopDownCharacterAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    public AuthoringKinematicCharacterBody CharacterBody = AuthoringKinematicCharacterBody.GetDefault();
    public TopDownCharacterComponent TopDownCharacter = TopDownCharacterComponent.GetDefault();

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        KinematicCharacterUtilities.HandleConversionForCharacter(dstManager, entity, gameObject, CharacterBody);

        dstManager.AddComponentData(entity, TopDownCharacter);
        dstManager.AddComponentData(entity, new TopDownCharacterInputs());
    }
}
