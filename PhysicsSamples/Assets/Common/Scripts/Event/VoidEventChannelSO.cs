using UnityEngine;
using UnityEngine.Events;
#if USE_ODIN
using Sirenix.OdinInspector;
#endif
/// <summary>
/// This class is used for Events that have no arguments (Example: Exit game event)
/// </summary>

public class VoidEventChannelSO : DescriptionBaseSO
{
    [SerializeField]
    public UnityAction OnEventRaised;
#if USE_ODIN
    [Button("RaiseEvent")]
    //public int Play;
#endif
    public void RaiseEvent()
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke();
    }
}
