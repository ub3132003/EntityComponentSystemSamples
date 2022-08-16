using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using Sirenix.OdinInspector;

public class RpgStatSO : ThingSO
{
    [Header("-----BASE DATA-----")]
    public int ID = -1;

    [FoldoutGroup("Base Info")]
    public string _fileName;
    [FoldoutGroup("Base Info")]
    public string displayName;
    //public string description;


    [FoldoutGroup("Setup")]
    public bool minCheck;
    [FoldoutGroup("Setup")][ShowIf("minCheck")]
    public float minValue;
    [FoldoutGroup("Setup")]
    public bool maxCheck;
    [FoldoutGroup("Setup")]
    [ShowIf("maxCheck")]
    public float maxValue;
    [FoldoutGroup("Setup")]
    public float baseValue;

    //[FoldoutGroup("Setup")]
    //public bool isShiftingInSprint = true;
    //[FoldoutGroup("Setup")]
    //public bool isShiftingInBlock = true;
    //[FoldoutGroup("Setup")]
    //public bool isShiftingOutsideCombat;
    //[FoldoutGroup("Setup")]
    //public bool isShiftingInCombat;
    //[FoldoutGroup("Setup")]
    //public float shiftAmountOutsideCombat;
    //[FoldoutGroup("Setup")]
    //public float shiftIntervalOutsideCombat;
    //[FoldoutGroup("Setup")]
    //public float shiftAmountInCombat;
    //[FoldoutGroup("Setup")]
    //public float shiftIntervalInCombat;
    [FoldoutGroup("Setup")]
    public string StatUICategory;

    [FoldoutGroup("Setup")]
    [ShowIf("isVitalityStat")]
    public bool StartsAtMax = true;

    [FoldoutGroup("Setup")]
    [ShowIf("isVitalityStat")]
    public bool isPercentStat;


    public enum VitalityActionsTypes
    {
        Death,
        Effect,
        Ability,
        ResetActiveBlock,
        ResetSprint
    }

    public enum VitalityActionsValueType
    {
        Equal,
        EqualOrBelow,
        EqualOrAbove,
        Below,
        Above
    }

    [FoldoutGroup("Setup")]
    public bool isVitalityStat;

    [Serializable]
    public class VitalityActions
    {
        public VitalityActionsTypes type;
        public float value;
        public VitalityActionsValueType valueType;
        public bool isPercent;

        public int elementID = -1;
        public int itemCount;
    }
    [ShowIf("isVitalityStat"), FoldoutGroup("Vitality Actions")]
    public List<VitalityActions> vitalityActions = new List<VitalityActions>();

    [Serializable]
    public class OnHitEffectsData
    {
        public RpgEffectSO effectREF;
        public int effectID = -1;
        public int effectRank;
        public float chance = 100f;
    }

    public enum STAT_TYPE
    {
        NONE,
        DAMAGE,
        RESISTANCE,
        PENETRATION,
        HEALING,
        ABSORBTION,
        CC_POWER,
        CC_RESISTANCE,
        DMG_TAKEN,
        DMG_DEALT,
        HEAL_RECEIVED,
        HEAL_DONE,
        CAST_SPEED,
        CRIT_CHANCE,
        BASE_DAMAGE_TYPE,
        BASE_RESISTANCE_TYPE,
        SUMMON_COUNT,
        CD_RECOVERY_SPEED,
        GLOBAL_HEALING,
        LIFESTEAL,
        THORN,
        BLOCK_CHANCE,
        BLOCK_FLAT,
        BLOCK_MODIFIER,
        DODGE_CHANCE,
        GLANCING_BLOW_CHANCE,
        CRIT_POWER,
        DOT_BONUS,
        HOT_BONUS,
        HEALTH_ON_HIT,
        HEALTH_ON_KILL,
        HEALTH_ON_BLOCK,
        EFFECT_TRIGGER,
        LOOT_CHANCE_MODIFIER,
        EXPERIENCE_MODIFIER,
        VITALITY_REGEN,
        MINION_DAMAGE,
        MINION_PHYSICAL_DAMAGE,
        MINION_MAGICAL_DAMAGE,
        MINION_HEALTH,
        MINION_CRIT_CHANCE,
        MINION_CRIT_POWER,
        MINION_DURATION,
        MINION_LIFESTEAL,
        MINION_THORN,
        MINION_DODGE_CHANCE,
        MINION_GLANCING_BLOW_CHANCE,
        MINION_HEALTH_ON_HIT,
        MINION_HEALTH_ON_KILL,
        MINION_HEALTH_ON_BLOCK,
        PROJECTILE_SPEED,
        PROJECTILE_ANGLE_SPREAD,
        PROJECTILE_RANGE,
        PROJECTILE_COUNT,
        AOE_RADIUS,
        ABILITY_MAX_HIT,
        ABILITY_TARGET_MAX_RANGE,
        ABILITY_TARGET_MIN_RANGE,
        ATTACK_SPEED,
        BODY_SCALE,
        GCD_DURATION,
        BLOCK_ACTIVE_ANGLE,
        BLOCK_ACTIVE_COUNT,
        BLOCK_ACTIVE_CHARGE_TIME_SPEED_MODIFIER,
        BLOCK_ACTIVE_DURATION_MODIFIER,
        BLOCK_ACTIVE_DECAY_MODIFIER,
        BLOCK_ACTIVE_FLAT_AMOUNT,
        BLOCK_ACTIVE_PERCENT_AMOUNT,
        MOVEMENT_SPEED
    }

    [Serializable]
    public class StatBonusData
    {
        public STAT_TYPE statType;
        public float modifyValue = 1;
        [ShowIf("@statType == STAT_TYPE.DAMAGE||statType == STAT_TYPE.RESISTANCE")]
        public string StatFunction;
        [ShowIf("@statType == STAT_TYPE.DAMAGE||statType == STAT_TYPE.RESISTANCE")]
        public string OppositeStat;

        public int statID = -1;
    }
    /// <summary>
    /// 用于计算属性的值
    /// </summary>
    [HideIf("isVitalityStat"), FoldoutGroup("Bonuss")]
    public List<StatBonusData> statBonuses = new List<StatBonusData>();

    //触发特效,中毒机率 击退机率
    [ShowIf("@!isVitalityStat")]
    public List<OnHitEffectsData> onHitEffectsData = new List<OnHitEffectsData>();
}
