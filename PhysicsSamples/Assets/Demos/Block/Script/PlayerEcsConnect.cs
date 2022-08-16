using System.Collections;
using UnityEngine;


public class PlayerEcsConnect : Singleton<PlayerEcsConnect>
{
    /// <summary>
    /// 经验值,目前死亡方块数量
    /// </summary>

    public IntEventChannelSO ExpChangeEvent;
    public IntEventChannelSO LevelDisplayEvent;

    [SerializeField]
    PlayerLevel playerLevel = new PlayerLevel();

    [SerializeField]
    LevelCompoent levelCompoent = new LevelCompoent();

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
                    ClassLevelUp();
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

        public void ClassLevelUp()
        {
        }
    }

    public void AddEXP(int exp)
    {
        playerLevel.AddClassXP(levelCompoent, exp);
    }
}
