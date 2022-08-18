using System.Collections;
using System.Collections.Generic;
using UnityEngine;


public class CharacterData : Singleton<CharacterData>
{
    public class Bonus_DATA
    {
        public RPGBonus BonusRef;
        public int ID;
        public bool known;
        public int rank;
        public bool On;
    }
    /// <summary>
    /// 被动技能实体
    /// </summary>
    public List<Bonus_DATA> bonusesData;

    [System.Serializable]
    public class BONUS_LearnedDATA
    {
        public int bonusID;
    }

    public List<BONUS_LearnedDATA> bonusLearned;

    public static int GetRankFromCharacterData(RPGBonus bonus)
    {
        var b = CharacterData.Instance.bonusesData;
        foreach (var item in b)
        {
            if (item.BonusRef == bonus)
            {
                return item.rank;
            }
        }
        return -1;
    }
}
