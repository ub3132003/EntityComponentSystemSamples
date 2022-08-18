using System;
using System.Collections.Generic;

using UnityEngine;

public class RPGBonus : ThingSO
{
    public int ID = -1;

    public string displayName;
    public string _fileName;

    public bool learnedByDefault;

    [Serializable]
    public class RPGBonusRankDATA
    {
        public bool ShowedInEditor;
        public int unlockCost;

        public List<RpgEffectSO.STAT_EFFECTS_DATA> statEffectsData = new List<RpgEffectSO.STAT_EFFECTS_DATA>();
    }
    public List<RPGBonusRankDATA> ranks = new List<RPGBonusRankDATA>();
}
