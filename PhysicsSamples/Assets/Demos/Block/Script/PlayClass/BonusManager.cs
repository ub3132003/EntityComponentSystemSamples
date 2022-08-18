using System.Collections;
using UnityEngine;


public class BonusManager
{
    public static void BonusLearned(RPGBonus bonus)
    {
        var newDATA = new CharacterData.BONUS_LearnedDATA();
        newDATA.bonusID = bonus.ID;
        CharacterData.Instance.bonusLearned.Add(newDATA);
    }

    private static CharacterData.Bonus_DATA getBonusDATAByBonus(RPGBonus bonus)
    {
        foreach (var t in CharacterData.Instance.bonusesData)
            if (t.ID == bonus.ID) return t;

        return null;
    }

    public static void InitBonus(RPGBonus ab)
    {
        var bnsDATA = getBonusDATAByBonus(ab);
        if (!bnsDATA.known || bnsDATA.On) return;
        var curRank = bnsDATA.rank;
        if (curRank < 0) return;
        HandleBonusActions(ab);
    }

    public static void RankUpBonus(RPGBonus bonus)
    {
        foreach (var t in CharacterData.Instance.bonusesData)
        {
            if (t.BonusRef != bonus) continue;
            if (t.rank >= bonus.ranks.Count) continue;

            var rankREF = bonus.ranks[t.rank];


            t.rank++;
            t.known = true;


            if (t.rank == 1)
            {
                BonusLearned(bonus);
                InitBonus(bonus);
            }
            else if (t.rank > 1)
            {
                var previousRank = -1;
                previousRank += t.rank - 1;
                CancelBonus(bonus, previousRank);
                InitBonus(bonus);
            }
        }
    }

    private static void AlterBonusState(RPGBonus bonus, bool isOn)
    {
        foreach (var bns in CharacterData.Instance.bonusesData)
            if (bns.BonusRef == bonus) bns.On = isOn;
    }

    private static void CancelBonus(RPGBonus ab, int curRank)
    {
        AlterBonusState(ab, false);
        StatCalculator.CalculateBonusStats();
    }

    private static void HandleBonusActions(RPGBonus bonus)
    {
        AlterBonusState(bonus, true);
        StatCalculator.CalculateBonusStats();
    }
}
