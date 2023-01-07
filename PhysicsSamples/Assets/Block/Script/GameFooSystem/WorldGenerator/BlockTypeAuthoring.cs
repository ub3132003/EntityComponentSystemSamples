using Unity.Entities;

// Authoring MonoBehaviours are regular GameObject components.
// They constitute the inputs for the baking systems which generates ECS data.
public class BlockTypeAuthoring : UnityEngine.MonoBehaviour, IConvertGameObjectToEntity
{
    public UnityEngine.GameObject sixSidedPrefab;
    public UnityEngine.GameObject defaultPrefab;
    public UnityEngine.GameObject defaultAlphaPrefab;
    public UnityEngine.GameObject plantPrefab;

    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        dstManager.AddComponentData(entity, new BlockType
        {
            //sixSidedPrefab = GetEntity(this.sixSidedPrefab),
            //defaultPrefab = GetEntity(this.defaultPrefab),
            //defaultAlphaPrefab = GetEntity(this.defaultAlphaPrefab),
            //plantPrefab = GetEntity(this.plantPrefab)
            sixSidedPrefab = GameEntityAssetManager.Instance.GetPrimaryEntity(sixSidedPrefab),
            defaultPrefab = GameEntityAssetManager.Instance.GetPrimaryEntity(defaultPrefab),
            defaultAlphaPrefab = GameEntityAssetManager.Instance.GetPrimaryEntity(defaultAlphaPrefab),
            plantPrefab = GameEntityAssetManager.Instance.GetPrimaryEntity(plantPrefab),
        });
    }
}
