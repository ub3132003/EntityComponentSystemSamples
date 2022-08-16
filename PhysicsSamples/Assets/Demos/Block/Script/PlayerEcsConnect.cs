using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using UnityEngine;


public class PlayerEcsConnect : Singleton<PlayerEcsConnect>
{
    /// <summary>
    /// 经验值,目前死亡方块数量
    /// </summary>


    [SerializeField]
    PlayerLevel playerLevel = new PlayerLevel();

    [SerializeField]
    LevelCompoent levelCompoent = new LevelCompoent();

    [Header("boardcast on")]
    public IntEventChannelSO ExpChangeEvent;
    public IntEventChannelSO LevelDisplayEvent;


    private void Update()
    {
        ExpChangeEvent.RaiseEvent(levelCompoent.currentXP);
        LevelDisplayEvent.RaiseEvent(levelCompoent.currentLevel);
    }

    [System.Serializable]
    public class LevelCompoent
    {
        public RpgLevelTemplateSO LevelTemplate;
        public int currentLevel;
        public int currentXP, maxXP = 1;
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
                    charactorLv.maxXP = charactorLv.LevelTemplate.allLevels[charactorLv.currentLevel - 1].XPRequired;

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

    public void AddEXP(int exp)
    {
        playerLevel.AddClassXP(levelCompoent, exp);
    }

    private EntityManager entityManager;
    private Entity player;
    private void Start()
    {
        entityManager = World.DefaultGameObjectInjectionWorld.EntityManager;
        //TOdo 等待player
    }

    public void RegistPlayer(Entity player)
    {
        this.player = player;
    }

    public class NODE_STATS
    {
        public string _name;
        public BarSkillItemSO stat;
        public float curMinValue;
        public float curMaxValue;
        public float curValue;
        public float nextCombatShift, nextRestShift;
        public float valueFromItem;
        public float valueFromBonus;
        public float valueFromEffect;
        public float valueFromShapeshifting;
    }

    [SerializeField] List<NODE_STATS> nodeStats = new List<NODE_STATS>();
    public void SetEcsPlayer()
    {
        entityManager.GetComponentObject<FollowMouseOnGroud>(player);
    }

    public int getStatIndexFromName(string statname)
    {
        for (var i = 0; i < nodeStats.Count; i++)
            if (nodeStats[i]._name == statname)
                return i;
        return -1;
    }

    private void UpdateStat(string _name, string valueType, float Amount)
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
}
