using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;

using Unity.Transforms;
using Unity.Mathematics;
using Unity.Physics;

public class PlayerEcsConnect : Singleton<PlayerEcsConnect>, IReceiveEntity
{
    /// <summary>
    /// 经验值,目前死亡方块数量
    /// </summary>

    //辅助对象，跟随实体的位置
    [SerializeField]
    private FollowEnetityObj FollowEntityGameObject;

    [SerializeField]
    PlayerLevel playerLevel = new PlayerLevel();

    [SerializeField]
    LevelCompoent levelCompoent = new LevelCompoent();

    [Header("boardcast on")]
    public IntEventChannelSO ExpDisplayEvent;
    public IntEventChannelSO LevelDisplayEvent;


    public void AddEXP(int exp)
    {
        playerLevel.AddClassXP(levelCompoent, exp);
    }

    private EntityManager entityManager;
    private Entity player;

    [SerializeField]
    private CombatNode playerNode;

    public CombatNode PlayerNode
    {
        get =>  playerNode == null ? playerNode = new CombatNode() : playerNode;

        set => playerNode = value;
    }
    public Entity Player { get => player; set => player = value; }

    List<Entity> gunEnties;
    public List<Entity> GunEnties => gunEnties;
    public EntityManager EntityManager { get => entityManager; set => entityManager = value; }

    [SerializeField] List<RpgStatSO> rpgStatSOs;
    protected override void Awake()
    {
        base.Awake();
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
    }

    IEnumerator Start()
    {
        //TOdo 等待player 生成

        playerNode.InitStats(rpgStatSOs);
        while (player == Entity.Null)
        {
            yield return 0;
        }
        //查找所有gun 实体
        var gunSystem = World.DefaultGameObjectInjectionWorld.GetExistingSystem<CharacterGunOneToManyInputSystem>();
        EntityQueryDesc description = new EntityQueryDesc
        {
            All = new ComponentType[]
            {
                typeof(CharacterGun),
            }
        };
        var queryBuilder = new EntityQueryDescBuilder(Unity.Collections.Allocator.TempJob);
        queryBuilder.AddAll(typeof(CharacterGun));
        queryBuilder.FinalizeQuery();

        EntityQuery gunGroup = gunSystem.GetEntityQuery(queryBuilder);

        var guns = gunGroup.ToEntityArray(Unity.Collections.Allocator.TempJob);
        gunEnties = new List<Entity>(guns);

        queryBuilder.Dispose();
        guns.Dispose();

        Debug.Log("Player Regist");
    }

    private void Update()
    {
        ExpDisplayEvent.RaiseEvent(levelCompoent.currentXP);
        LevelDisplayEvent.RaiseEvent(levelCompoent.currentLevel);

        PlayerNode.UpdateStates();

        UpdateEntity();
    }

    public void RegistPlayer(Entity player)
    {
        this.player = player;
        FollowEntityGameObject.SetReceivedEntity(player);
    }

    public void RotatePlayerTo(Vector3 dir)
    {
        quaternion rotation = quaternion.LookRotation(dir, math.up());
        entityManager.SetComponentData<Rotation>(player, new Rotation
        {
            Value = rotation
        });
    }

    public Vector3 GetPlayerPosition()
    {
        return entityManager.GetComponentData<LocalToWorld>(player).Position;
    }

    public void AddBuff(RpgEffectSO rpgEffectSO, int rank)
    {
        PlayerNode.InitNodeState(PlayerNode, PlayerNode, rpgEffectSO, rank);
    }

    //升级被动


    public void UpdateEntity()
    {
        //写回实体系统
        if (player != Entity.Null)
        {
            var stats = playerNode.nodeStats;
            foreach (var item in stats)
            {
                foreach (var bonuses in item.stat.statBonuses)
                {
                    switch (bonuses.statType)
                    {
                        case RpgStatSO.STAT_TYPE.MOVEMENT_SPEED:
                            break;
                        case RpgStatSO.STAT_TYPE.BODY_SCALE:
                            //entityManager.SetComponentData(player, new NonUniformScale
                            //{
                            //    Value = new float3(item.curValue, 1, 1)
                            //});
                            //entityManager.SetComponentData(player, new Scale
                            //{
                            //    Value = item.curValue
                            //});
                            var scale = entityManager.GetComponentData<CompositeScale>(player);
                            scale.Value = float4x4.Scale(item.curValue, 1, 1);
                            entityManager.SetComponentData(player, scale);
                            var collider = entityManager.GetComponentData<PhysicsCollider>(player);
                            unsafe
                            {
                                Unity.Physics.BoxCollider* bcPtr = (Unity.Physics.BoxCollider*)collider.ColliderPtr;
                                var boxGeometry = bcPtr->Geometry;
                                boxGeometry.Size = new float3(item.curValue, 1, 1);
                                bcPtr->Geometry = boxGeometry;
                            }

                            break;


                        default:
                            break;
                    }
                }
            }
        }
    }

    public void SetReceivedEntity(Entity entity)
    {
        RegistPlayer(entity);
    }

    #region static
    //有 bug
    public static Entity CreateEntityFormObj(GameObject prefab)
    {
        var settings = GameObjectConversionSettings.FromWorld(World.DefaultGameObjectInjectionWorld, null);
        var entity  = GameObjectConversionUtility.ConvertGameObjectHierarchy(prefab, settings);
        return World.DefaultGameObjectInjectionWorld.EntityManager.Instantiate(entity);
    }

    #endregion
}

[System.Serializable]
public class LevelCompoent
{
    public RpgLevelTemplateSO LevelTemplate;
    public int currentLevel;
    public int currentXP;
    public int maxXP => LevelTemplate.allLevels[currentLevel].XPRequired;
}
[System.Serializable]
public class PlayerLevel
{
    [SerializeField] IntEventChannelSO UpdateLevelEvent;
    public void AddClassXP(LevelCompoent charactorLv, int _amount)
    {
        if (charactorLv.currentLevel > charactorLv.LevelTemplate.Maxlevel)
        {
            return;
        }


        float totalAmt = _amount;

        while (totalAmt > 0)
        {
            var XPRemaining = charactorLv.maxXP -
                charactorLv.currentXP;
            //升级
            if (totalAmt > XPRemaining)
            {
                charactorLv.currentXP = 0;
                totalAmt -= XPRemaining;
                charactorLv.currentLevel++;
                //charactorLv.maxXP = charactorLv.LevelTemplate.allLevels[charactorLv.currentLevel - 1].XPRequired;

                // EXECUTE POINTS GAIN REQUIREMENTS
                ClassLevelUp(charactorLv);
                //if (levelUpGO != null)
                //{
                //    SpawnLevelUpGO();
                //}
            }
            //未升级,小于等于最大经验
            else
            {
                charactorLv.currentXP += (int)totalAmt;
                totalAmt = 0;
            }
        }
    }

    public void ClassLevelUp(LevelCompoent charactorLv)
    {
        UpdateLevelEvent.RaiseEvent(charactorLv.currentLevel);
    }
}

[System.Serializable]
public class CombatNode
{
    [Serializable]
    public class NodeStatesDATA
    {
        public string stateName;
        public CombatNode casterNode;
        public RpgEffectSO stateEffect;
        public int effectRank;

        public Sprite stateIcon;
        public int maxPulses;
        public int curPulses;
        public float nextPulse;
        public float pulseInterval;
        public float stateMaxDuration;
        public float stateCurDuration;
        public int curStack;
        public int maxStack;
        public GameObject stateEffectGO;
    }
    /// <summary>
    /// buff 状态
    /// </summary>
    public List<NodeStatesDATA> nodeStateData = new List<NodeStatesDATA>();
    /// <summary>
    /// 叠加计算bug , 相同buff的不同等级之间不能叠加, 叠加类型要小心,用等级代替叠加
    /// </summary>
    [Serializable]
    public class NODE_STATS
    {
        public string _name;
        public RpgStatSO stat;
        public float curMinValue;
        public float curMaxValue;
        public float curValue;
        public float nextCombatShift, nextRestShift;
        public float valueFromItem;
        public float valueFromBonus;
        public float valueFromEffect;
        public float valueFromShapeshifting;
    }

    public List<NODE_STATS> nodeStats = new List<NODE_STATS>();
    /// <summary>
    /// 更新buff
    /// </summary>
    #region 能力值
    public void InitStats(List<RpgStatSO> rpgStatSOs)
    {
        foreach (var t in rpgStatSOs)
        {
            var newAttributeToLoad = new NODE_STATS();
            newAttributeToLoad._name = t.Name.GetLocalizedString();
            newAttributeToLoad.stat = t;

            newAttributeToLoad.curMinValue = t.minValue;
            newAttributeToLoad.curValue = t.baseValue;
            newAttributeToLoad.curMaxValue = t.maxValue;


            nodeStats.Add(newAttributeToLoad);
        }
    }

    public void UpdateStat(string _name, string valueType, float Amount)
    {
        var statIndex = getStatIndexFromName(_name);
        if (statIndex == -1) return;
        float newValue = 0;
        bool triggerVitalityActions = false;
        switch (valueType)
        {
            case "curMin":
                newValue = nodeStats[statIndex].curMinValue += Amount;
                nodeStats[statIndex].curMinValue = newValue;
                break;
            case "curBase":
                newValue = nodeStats[statIndex].curValue += Amount;
                nodeStats[statIndex].curValue = newValue;
                triggerVitalityActions = nodeStats[statIndex].stat.isVitalityStat;
                break;
            case "curMax":
                newValue = nodeStats[statIndex].curMaxValue += Amount;
                nodeStats[statIndex].curMaxValue = newValue;
                break;
            case "defaultMin":
                nodeStats[statIndex].stat.minValue = newValue;
                break;
            case "defaultBase":
                nodeStats[statIndex].stat.baseValue = newValue;
                break;
            case "defaultMax":
                nodeStats[statIndex].stat.maxValue = newValue;
                break;
        }
    }

    public int getStatIndexFromName(string statname)
    {
        for (var i = 0; i < nodeStats.Count; i++)
            if (nodeStats[i]._name == statname)
                return i;
        return -1;
    }

    #endregion


    #region  buff

    public void UpdateStates()
    {
        for (var i = 0; i < nodeStateData.Count; i++)
        {
            if (!nodeStateData[i].stateEffect.endless) nodeStateData[i].stateCurDuration += Time.deltaTime;
            if (nodeStateData[i].curPulses > 0)
            {
                nodeStateData[i].nextPulse -= Time.deltaTime;
            }

            if (nodeStateData[i].nextPulse <= 0 && nodeStateData[i].curPulses < nodeStateData[i].maxPulses)
            {
                nodeStateData[i].nextPulse = nodeStateData[i].pulseInterval;
                nodeStateData[i].curPulses++;

                switch (nodeStateData[i].stateEffect.effectType)
                {
                    case RpgEffectSO.EFFECT_TYPE.DamageOverTime:

                        break;
                    case RpgEffectSO.EFFECT_TYPE.HealOverTime:

                        break;
                    case RpgEffectSO.EFFECT_TYPE.Stat:
                        StatCalculator.CalculateEffectsStats(this);
                        break;
                }

                if (i + 1 > nodeStateData.Count) return;
            }

            if (nodeStateData[i].stateEffect.endless || !(nodeStateData[i].stateCurDuration >= nodeStateData[i].stateMaxDuration)) continue;
            //其他状态
            //HandleEffectEnd(i);
            return;
        }
    }

    private static CombatNode.NodeStatesDATA GenerateNewState(RpgEffectSO effect, int effectRank, CombatNode casterInfo, Sprite icon)
    {
        return new CombatNode.NodeStatesDATA
        {
            stateName = effect.Name.GetLocalizedString(),
            casterNode = casterInfo,
            stateMaxDuration = effect.duration,
            stateCurDuration = 0,
            curStack = 1,
            maxStack = effect.stackLimit,
            stateEffect = effect,
            effectRank = effectRank,
            stateIcon = icon
        };
    }

    /// <summary>
    /// 添加新buff
    /// </summary>
    /// <param name="effect"></param>
    /// <param name="effectRank"></param>
    /// <param name="casterInfo"></param>
    /// <param name="icon"></param>
    /// <param name="targetInfo"></param>
    public void InitNewStateEffect(RpgEffectSO effect, int effectRank, CombatNode casterInfo, CombatNode targetInfo)
    {
        var newState = GenerateNewState(effect, effectRank, casterInfo, effect.PreviewImage);

        newState.curPulses = 0;
        newState.maxPulses = effect.pulses;
        newState.pulseInterval = effect.duration / effect.pulses;

        targetInfo.nodeStateData.Add(newState);
    }

    private bool isEffectCC(RpgEffectSO.EFFECT_TYPE effectType)
    {
        return effectType == RpgEffectSO.EFFECT_TYPE.Stun
            || effectType == RpgEffectSO.EFFECT_TYPE.Root
            || effectType == RpgEffectSO.EFFECT_TYPE.Silence
            || effectType == RpgEffectSO.EFFECT_TYPE.Sleep
            || effectType == RpgEffectSO.EFFECT_TYPE.Taunt;
    }

    private bool isEffectUnique(RpgEffectSO.EFFECT_TYPE effectType)
    {
        return effectType == RpgEffectSO.EFFECT_TYPE.Immune
            || effectType == RpgEffectSO.EFFECT_TYPE.Stun
            || effectType == RpgEffectSO.EFFECT_TYPE.Root
            || effectType == RpgEffectSO.EFFECT_TYPE.Silence
            || effectType == RpgEffectSO.EFFECT_TYPE.Sleep
            || effectType == RpgEffectSO.EFFECT_TYPE.Taunt
            || effectType == RpgEffectSO.EFFECT_TYPE.Shapeshifting
            || effectType == RpgEffectSO.EFFECT_TYPE.Flying
            || effectType == RpgEffectSO.EFFECT_TYPE.Stealth;
    }

    private bool isEffectState(RpgEffectSO.EFFECT_TYPE effectType)
    {
        return effectType == RpgEffectSO.EFFECT_TYPE.DamageOverTime
            || effectType == RpgEffectSO.EFFECT_TYPE.HealOverTime
            || effectType == RpgEffectSO.EFFECT_TYPE.Stat
            || effectType == RpgEffectSO.EFFECT_TYPE.Shapeshifting
            || effectType == RpgEffectSO.EFFECT_TYPE.Flying
            || effectType == RpgEffectSO.EFFECT_TYPE.Stealth;
    }

    /// <summary>
    /// 计算buff
    /// </summary>
    /// <param name="casterInfo"></param>
    /// <param name="targetInfo"></param>
    /// <param name="effect"></param>
    /// <param name="effectRank"></param>
    /// <param name="delay"></param>
    public void InitNodeState(CombatNode casterInfo, CombatNode targetInfo, RpgEffectSO effect, int effectRank)
    {
        var hasSameUniqueEffect = false;
        var hasSameState = false;
        var curStateIndex = -1;
        var allNodeStates = targetInfo.nodeStateData;


        for (var i = 0; i < allNodeStates.Count; i++)
            if (isEffectUnique(effect.effectType) && effect.effectType == allNodeStates[i].stateEffect.effectType)
            {
                hasSameUniqueEffect = true;
                curStateIndex = i;
                break;
            }

        if (!hasSameUniqueEffect)
        {
            for (var i = 0; i < allNodeStates.Count; i++)
                if (isEffectState(effect.effectType) && effect == allNodeStates[i].stateEffect)
                {
                    hasSameState = true;
                    curStateIndex = i;
                    break;
                }
        }

        if (hasSameUniqueEffect)
        {
        }
        else if (hasSameState)
        {
            if (targetInfo.nodeStateData[curStateIndex].casterNode == casterInfo ||
                targetInfo.nodeStateData[curStateIndex].stateEffect.allowMixedCaster
            ) // same effect: from same caster || mixed caster is allowed
            {
                if (targetInfo.nodeStateData[curStateIndex].curStack < targetInfo.nodeStateData[curStateIndex].maxStack)
                    targetInfo.nodeStateData[curStateIndex].curStack++;
                else
                {
                    if (effect.allowMultiple)
                    {
                        InitNewStateEffect(effect, effectRank, casterInfo, targetInfo);
                        return;
                    }
                }

                // REFRESH THE EFFECT
                targetInfo.nodeStateData[curStateIndex].curPulses = 0;
                targetInfo.nodeStateData[curStateIndex].nextPulse = 0;
                targetInfo.nodeStateData[curStateIndex].stateCurDuration = 0;
            }
            else if (targetInfo.nodeStateData[curStateIndex].stateEffect.allowMultiple)
            {
                // caster is: not same || mixed caster is not allowed
                // we add it as a new effect
                InitNewStateEffect(effect, effectRank, casterInfo, targetInfo);
            }
        }
        else
        {
            InitNewStateEffect(effect, effectRank, casterInfo, targetInfo);

            var newState = GenerateNewState(effect, effectRank, casterInfo, effect.PreviewImage);
        }
    }

    #endregion
}

/// <summary>
/// 状态计算工具
/// </summary>
public static class StatCalculator
{
    #region bouns
    public static void CalculateBonusStats()
    {
        tempStatList.Clear();
        ResetBonusStats();

        foreach (var t in CharacterData.Instance.bonusesData)
        {
            if (!t.known) continue;
            if (!t.On) continue;

            RPGBonus bonusREF = t.BonusRef;

            foreach (var t1 in bonusREF.ranks[t.rank].statEffectsData)
            {
                var statREF = t1.statREF;
                foreach (var t3 in PlayerEcsConnect.Instance.PlayerNode.nodeStats)
                {
                    if (t3.stat.Name != statREF.Name) continue;
                    HandleStat(PlayerEcsConnect.Instance.PlayerNode, statREF, t3, t1.statEffectModification, t1.isPercent,
                        TemporaryStatSourceType.bonus);
                }
            }
        }
    }

    private static void ResetBonusStats()
    {
        foreach (var t2 in PlayerEcsConnect.Instance.PlayerNode.nodeStats)
        {
            if (t2.valueFromBonus == 0) continue;
            if (t2.stat.isVitalityStat)
            {
                t2.curMaxValue -= t2.valueFromBonus;
            }
            else
            {
                t2.curValue -= t2.valueFromBonus;
            }
            t2.valueFromBonus = 0;
        }
    }

    #endregion
    private class TemporaryStatsDATA
    {
        public RpgStatSO stat;
        public float value;
    }

    private static List<TemporaryStatsDATA> tempStatList = new List<TemporaryStatsDATA>();
    public static void CalculateEffectsStats(CombatNode cbtNode)
    {
        void ResetEffectsStats(CombatNode cbtNode)
        {
            foreach (var t2 in cbtNode.nodeStats)
            {
                if (t2.valueFromEffect == 0) continue;
                if (t2.stat.isVitalityStat)
                {
                    t2.curMaxValue -= t2.valueFromEffect;
                }
                else
                {
                    t2.curValue -= t2.valueFromEffect;
                }

                t2.valueFromEffect = 0;
                ClampStat(t2.stat, cbtNode);
            }
        }

        tempStatList.Clear();
        ResetEffectsStats(cbtNode);

        foreach (var t in cbtNode.nodeStateData)
        {
            if (t.stateEffect.effectType != RpgEffectSO.EFFECT_TYPE.Stat) continue;
            foreach (var t1 in t.stateEffect.ranks[t.effectRank].statEffectsData)
            {
                var statREF = t1.statREF;
                foreach (var t3 in cbtNode.nodeStats)
                {
                    if (t3.stat.Name != statREF.Name) continue;
                    HandleStat(cbtNode, statREF, t3, t1.statEffectModification * t.curStack, t1.isPercent, TemporaryStatSourceType.effect);
                }
            }
        }

        //ProcessTempStatList(TemporaryStatSourceType.effect, cbtNode);


        //UpdateStatUI();
    }

    public static void HandleStat(CombatNode cbtNode, RpgStatSO statREF, CombatNode.NODE_STATS nodeStatData, float amount, bool isPercent, TemporaryStatSourceType sourceType)
    {
        float addedValue = amount;
        if (isPercent)
        {
            tempStatList = AddStatsToTemp(tempStatList, statREF, addedValue);
            return;
        }

        if (statREF.isVitalityStat)
        {
            //float statOverride = GameModifierManager.Instance.GetStatOverrideModifier(RPGGameModifier.CategoryType.Combat + "+" +
            //    RPGGameModifier.CombatModuleType.Stat + "+" +
            //    RPGGameModifier.StatModifierType.MaxOverride, statREF.ID);
            //if (statOverride != -1)
            //{
            //    nodeStatData.curMaxValue = statOverride;
            //}
            //else
            //{
            //    nodeStatData.curMaxValue += addedValue;
            //}
        }
        else
        {
            nodeStatData.curValue += addedValue;
        }

        switch (sourceType)
        {
            case TemporaryStatSourceType.item:
                nodeStatData.valueFromItem += addedValue;
                break;
            case TemporaryStatSourceType.effect:
                nodeStatData.valueFromEffect += addedValue;
                break;
            case TemporaryStatSourceType.bonus:
                nodeStatData.valueFromBonus += addedValue;
                break;
            case TemporaryStatSourceType.shapeshifting:
                nodeStatData.valueFromShapeshifting += addedValue;
                break;
        }

        ClampStat(statREF, cbtNode);


        //if (cbtNode == CombatManager.playerCombatNode && RPGBuilderUtilities.isStatAffectingMoveSpeed(statREF)) TriggerMoveSpeedChange(); //移速涉及动画等一系列修改
        //if (cbtNode.nodeType == CombatNode.COMBAT_NODE_TYPE.mob || cbtNode.nodeType == CombatNode.COMBAT_NODE_TYPE.pet) TriggerNPCMoveSpeedChange(cbtNode); // 设计ai的一系列移动计算
    }

    public enum TemporaryStatSourceType
    {
        none,
        item,
        effect,
        bonus,
        shapeshifting
    }
    private static List<TemporaryStatsDATA> AddStatsToTemp(List<TemporaryStatsDATA> tempList, RpgStatSO statREF, float value)
    {
        foreach (var t in tempList)
        {
            if (t.stat != statREF) continue;
            t.value += value;
            return tempList;
        }

        TemporaryStatsDATA newTempStatData = new TemporaryStatsDATA();
        newTempStatData.stat = statREF;
        newTempStatData.value = value;
        tempList.Add(newTempStatData);
        return tempList;
    }

    public static void ClampStat(RpgStatSO stat, CombatNode cbtNode)
    {
        CombatNode.NODE_STATS nodeStat = cbtNode.nodeStats[cbtNode.getStatIndexFromName(stat.Name.GetLocalizedString())];
        if (stat.minCheck && nodeStat.curValue < getMinValue(nodeStat))
        {
            nodeStat.curValue = (int)nodeStat.curMinValue;
        }
        if (stat.maxCheck && nodeStat.curValue > getMaxValue(nodeStat))
        {
            nodeStat.curValue = (int)nodeStat.curMaxValue;
        }
    }

    private static float getMinValue(CombatNode.NODE_STATS nodeStat)
    {
        return nodeStat.curMinValue;
    }

    private static float getMaxValue(CombatNode.NODE_STATS nodeStat)
    {
        return nodeStat.curMaxValue;
    }
}
