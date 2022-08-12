using UnityEngine;
using UnityEditor;
#if USE_ODIN
using Sirenix.OdinInspector;
#endif

public class SerializableScriptableObject : ScriptableObject
{
#if USE_ODIN
    [readonly]
#endif
    [SerializeField] private string _guid;

#if USE_ODIN
    [ShowInInspector]
#endif
    public string Guid => _guid;

#if UNITY_EDITOR
    void OnValidate()
    {
        var path = AssetDatabase.GetAssetPath(this);
        _guid = AssetDatabase.AssetPathToGUID(path);
    }

#endif
}
