using System;
using System.Collections.Generic;

using UnityEngine;
using UnityEngine.Events;

using Sirenix.OdinInspector;

static class EditorGroupName
{
    public const string baseName = "BASE INFO";
    public const string type = "TYPE";
    public const string state = "STATE SETTING";
    public const string blocking = "BLOCK";
    public const string motion = "Motion";
    public const string teleport = "Teleport";
}

public class RpgEffectSO : ThingSO
{
    [FoldoutGroup(EditorGroupName.baseName)]
    public int ID = -1;
    [FoldoutGroup(EditorGroupName.baseName)]
    public string _fileName;
    [FoldoutGroup(EditorGroupName.baseName)]
    public string displayName;


    public enum EFFECT_TYPE
    {
        InstantDamage,
        InstantHeal,
        DamageOverTime,
        HealOverTime,
        Stat,
        Stun,
        Sleep,
        Immune,
        Shapeshifting,
        Dispel,
        Teleport,
        Taunt,
        Root,
        Silence,
        Pet,
        RollLootTable,
        Knockback,
        Motion,
        Blocking,
        Flying,
        Stealth
    }

    public enum MAIN_DAMAGE_TYPE
    {
        NONE,
        PHYSICAL_DAMAGE,
        MAGICAL_DAMAGE
    }

    [Serializable]
    public class STAT_EFFECTS_DATA
    {
        public int statID = -1;
        public RpgStatSO statREF;
        public float statEffectModification = 1;
        public bool isPercent;
    }

    public enum TELEPORT_TYPE
    {
        gameScene,
        position,
        target,
        directional
    }

    public enum TELEPORT_DIRECTIONAL_TYPE
    {
        forward,
        left,
        right,
        backward,
        diagonalTopLeft,
        diagonalTopRight,
        diagonalBackwardLeft,
        diagonalBackwardRight
    }

    public enum PET_TYPE
    {
        combat
    }

    [FoldoutGroup(EditorGroupName.type)]
    public EFFECT_TYPE effectType;
    [FoldoutGroup(EditorGroupName.type)]
    public string effectTag;


    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public bool isState, isBuffOnSelf;
    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public int stackLimit = 1;
    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public bool allowMultiple, allowMixedCaster;
    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public int pulses = 1;
    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public float duration;
    [ShowIf("@effectType ==  EFFECT_TYPE.Stat || effectType == EFFECT_TYPE.DamageOverTime")]
    public bool endless;

    public enum BLOCK_DURATION_TYPE
    {
        Time,
        HoldKey
    }
    public enum BLOCK_END_TYPE
    {
        HitCount,
        MaxDamageBlocked,
        Stat
    }
    public enum DISPEL_TYPE
    {
        EffectType,
        EffectTag,
        Effect
    }
    public enum ON_BLOCK_ACTION_TYPE
    {
        Ability,
        Effect
    }

    [Serializable]
    public class ON_BLOCK_ACTION
    {
        public ON_BLOCK_ACTION_TYPE blockActionType;
        public int entryID = -1;
        //public RPGCombatDATA.TARGET_TYPE target;
        public int effectRank;
        public bool abMustBeKnown;
        public float chance = 100f, delay;
    }

    [Serializable]
    public class RPGEffectRankData
    {
        public static EFFECT_TYPE effectType;
        public bool ShowedInEditor;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public MAIN_DAMAGE_TYPE mainDamageType;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public string secondaryDamageType;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public int Damage;
        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public RpgStatSO alteredStatREF;

        public COST_TYPES hitValueType;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public bool FlatCalculation;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public bool CannotCrit;

        [ShowIf("effectType", Value = EFFECT_TYPE.InstantDamage)]
        public bool removeStealth = true;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public float skillModifier, weaponDamageModifier;

        [ShowIf("effectType", Value = EFFECT_TYPE.InstantDamage)]
        [ShowIf("@weaponDamageModifier>0")]
        public bool useWeapon1Damage = true, useWeapon2Damage = true;

        //public RPGSkill skillModifierREF;
        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public float lifesteal;

        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public float maxHealthModifier;
        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime")]
        public float missingHealthModifier;

        public float UltimateGain;
        public float delay;

        public int projectilesReflectedCount;

        /// <summary>
        /// 通常用于增伤buff， 如对燃烧敌人额外伤害
        /// </summary>
        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public RpgEffectSO requiredEffectREF;
        //public int requiredEffectID = -1;
        [ShowIf("@requiredEffectREF!=null")]
        public float requiredEffectDamageModifier;

        /// <summary>
        /// 通常用于属性增伤， 如防御加成技能?
        /// </summary>
        [ShowIf("@effectType == EFFECT_TYPE.InstantDamage || effectType == EFFECT_TYPE.DamageOverTime || effectType == EFFECT_TYPE.InstantHeal")]
        public RpgStatSO damageStatREF;
        //public int damageStatID = -1;
        [ShowIf("@damageStatREF!=null")]
        public float damageStatModifier;


        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        public TELEPORT_TYPE teleportType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        //public int gameSceneID = -1;
        //public RPGGameScene gameSceneREF;
        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        public Vector3 teleportPOS;
        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        public TELEPORT_DIRECTIONAL_TYPE teleportDirectionalType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        public float teleportDirectionalDistance;
        [ShowIf("effectType", Value = EFFECT_TYPE.Teleport)]
        public LayerMask teleportDirectionalBlockLayers;


        /*
        public PET_TYPE petType;
        public int petNPCDataID = -1;
        public float petDuration;
        public int petSPawnCount;
        public bool petScaleWithCharacter;*/
        [ShowIf("effectType", Value = EFFECT_TYPE.Knockback)]
        public float knockbackDistance;
        [ShowIf("effectType", Value = EFFECT_TYPE.Motion)]
        public float motionDistance = 5;
        [ShowIf("effectType", Value = EFFECT_TYPE.Motion)]
        public float motionSpeed = 0.5f;
        [ShowIf("effectType", Value = EFFECT_TYPE.Motion)]
        public Vector3 motionDirection;
        [ShowIf("effectType", Value = EFFECT_TYPE.Motion)]
        public bool motionIgnoreUseCondition, isImmuneDuringMotion;

        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public bool blockAnyDamage = true, blockPhysicalDamage, blockMagicalDamage, isBlockChargeTime,
                    isBlockLimitedDuration, isBlockPowerDecay, isBlockKnockback, blockStatDecay;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public float blockChargeTime = 0.5f, blockDuration = 1, blockPowerModifier = 100, blockPowerDecay = 0.1f, blockAngle = 90f, blockStatDecayInterval = 1;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public int blockPowerFlat, blockHitCount = 1, blockMaxDamage, blockStatDecayAmount = 1;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public BLOCK_DURATION_TYPE blockDurationType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public BLOCK_END_TYPE blockEndType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public int blockStatID = -1;
        [ShowIf("effectType", Value = EFFECT_TYPE.Blocking)]
        public List<string> blockedDamageTypes = new List<string>();

        [ShowIf("effectType", Value = EFFECT_TYPE.Dispel)]
        public DISPEL_TYPE dispelType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Dispel)]
        public EFFECT_TYPE dispelEffectType;
        [ShowIf("effectType", Value = EFFECT_TYPE.Dispel)]
        public string dispelEffectTag;
        //public int dispelEffectID = -1;


        [ShowIf("effectType", Value = EFFECT_TYPE.Stat)]
        public List<STAT_EFFECTS_DATA> statEffectsData = new List<STAT_EFFECTS_DATA>();
    }
    public List<RPGEffectRankData> ranks = new List<RPGEffectRankData>();


    private void OnValidate()
    {
        RPGEffectRankData.effectType = this.effectType;
    }
}
