using System;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 基础表情
/// </summary>
public class Basic : MonoBehaviour
{
#if UNITY_EDITOR
    public BasicExpressionsInfo m_info = new BasicExpressionsInfo();
#endif
    public AnimationClip m_clip;
    public List<string> m_baseExpressionName = new List<string>();
    public List<float> m_baseExpressionValue = new List<float>();
}

#if UNITY_EDITOR
/// <summary>
/// 编辑器信息
/// </summary>
[Serializable]//便于编辑器储存，可视化操作
public class BasicExpressionsInfo
{
    public bool m_foldoutAnimation;
    public bool m_foldoutPredefine;
    public List<FloatArr> m_baseExpressionFloat = new List<FloatArr>();
    public List<StringArr> m_baseExpressionString = new List<StringArr>();
    public List<int> m_baseExpressionIndex = new List<int>();
}
[Serializable]//便于编辑器储存，可视化操作
public class FloatArr { public float[] m_value; }
[Serializable]//便于编辑器储存，可视化操作
public class StringArr { public string[] m_value; }
#endif
 