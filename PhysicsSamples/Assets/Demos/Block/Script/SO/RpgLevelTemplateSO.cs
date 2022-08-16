using System;
using System.Collections.Generic;
using UnityEngine;
/// <summary>
/// 等级模板
/// </summary>
public class RpgLevelTemplateSO : DescriptionBaseSO
{
    public int ID = -1;
    public string _name;
    public string _fileName;
    public int Maxlevel;
    public int baseXPValue;
    public float increaseAmount;

    [Serializable]
    public class LEVELS_DATA
    {
        public string levelName;
        public int level;
        public int XPRequired;
    }

    public List<LEVELS_DATA> allLevels = new List<LEVELS_DATA>();

    public void updateThis(RpgLevelTemplateSO newData)
    {
        ID = newData.ID;
        _name = newData._name;
        _fileName = newData._fileName;
        Maxlevel = newData.Maxlevel;
        baseXPValue = newData.baseXPValue;
        increaseAmount = newData.increaseAmount;
        allLevels = newData.allLevels;
    }
}