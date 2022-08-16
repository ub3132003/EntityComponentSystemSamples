using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class is used for Events that have one int argument.
/// Example: An Achievement unlock event, where the int is the Achievement ID.
/// </summary>
#if USE_ODIN

#endif
using Sirenix.OdinInspector;
public class IntEventChannelSO : DescriptionBaseSO
{
    public UnityAction<int> OnEventRaised;

#if USE_ODIN

#endif
    [Button]
    public void RaiseEvent(int value)
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke(value);
    }
}
