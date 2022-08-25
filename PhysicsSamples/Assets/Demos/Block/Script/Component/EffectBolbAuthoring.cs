using Unity.Collections;
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;
struct EffectBolbAsset
{
    public int Value;
    public COST_TYPES HitType;

    public static BlobAssetReference<EffectBolbAsset> RegisterBlobAsset(RpgEffectSO data , int rank)
    {
        using (BlobBuilder blobBuilder = new BlobBuilder(Allocator.Temp))
        {
            ref EffectBolbAsset asset = ref blobBuilder.ConstructRoot<EffectBolbAsset>();
            asset.Value = data.ranks[rank].Damage;
            asset.HitType = data.ranks[rank].hitValueType;
            return blobBuilder.CreateBlobAssetReference<EffectBolbAsset>(Allocator.Persistent);
        }
    }
}


public class EffectBolbAuthoring : MonoBehaviour, IConvertGameObjectToEntity
{
    [SerializeField] RpgEffectSO effectSO;
    [SerializeField] int rank;
    public void Convert(Entity entity, EntityManager dstManager, GameObjectConversionSystem conversionSystem)
    {
        var buffBlob = EffectBolbAsset.RegisterBlobAsset(effectSO , rank);

        conversionSystem.BlobAssetStore.AddUniqueBlobAsset(ref buffBlob);

        switch (effectSO.effectType)
        {
            case RpgEffectSO.EFFECT_TYPE.InstantDamage:
                dstManager.AddComponentData(entity, new Damage());
                break;
            case RpgEffectSO.EFFECT_TYPE.InstantHeal:
                break;
            case RpgEffectSO.EFFECT_TYPE.DamageOverTime:
                break;
            case RpgEffectSO.EFFECT_TYPE.HealOverTime:
                break;
            case RpgEffectSO.EFFECT_TYPE.Stat:
                break;
            case RpgEffectSO.EFFECT_TYPE.Stun:
                break;
            case RpgEffectSO.EFFECT_TYPE.Sleep:
                break;
            case RpgEffectSO.EFFECT_TYPE.Immune:
                break;
            case RpgEffectSO.EFFECT_TYPE.Shapeshifting:
                break;
            case RpgEffectSO.EFFECT_TYPE.Dispel:
                break;
            case RpgEffectSO.EFFECT_TYPE.Teleport:
                break;
            case RpgEffectSO.EFFECT_TYPE.Taunt:
                break;
            case RpgEffectSO.EFFECT_TYPE.Root:
                break;
            case RpgEffectSO.EFFECT_TYPE.Silence:
                break;
            case RpgEffectSO.EFFECT_TYPE.Pet:
                break;
            case RpgEffectSO.EFFECT_TYPE.RollLootTable:
                break;
            case RpgEffectSO.EFFECT_TYPE.Knockback:
                break;
            case RpgEffectSO.EFFECT_TYPE.Motion:
                break;
            case RpgEffectSO.EFFECT_TYPE.Blocking:
                break;
            case RpgEffectSO.EFFECT_TYPE.Flying:
                break;
            case RpgEffectSO.EFFECT_TYPE.Stealth:
                break;
            default:
                break;
        }
    }
}
