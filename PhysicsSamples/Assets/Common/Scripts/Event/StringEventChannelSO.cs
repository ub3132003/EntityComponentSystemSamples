using UnityEngine;
using UnityEngine.Events;
#if USE_ODIN
using Sirenix.OdinInspector;
#endif
/// <summary>
/// This class is used for Events that have no arguments (Example: Exit game event)
/// </summary>

public class StringEventChannelSO : DescriptionBaseSO
{
    [SerializeField]
    public UnityAction<string> OnEventRaised;
#if USE_ODIN
    [Button("RaiseEvent")]
    //public int Play;
#endif
    public void RaiseEvent(string val)
    {
        OnEventRaised?.Invoke(val);
    }
}
