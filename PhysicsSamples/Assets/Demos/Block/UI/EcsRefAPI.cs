using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EcsRefAPI : MonoBehaviour
{
    public void AddSkillBuff(RpgEffectSO rpgEffectSO , int rank)
    {
        PlayerEcsConnect.Instance.AddBuff(rpgEffectSO, rank);
    }
}
