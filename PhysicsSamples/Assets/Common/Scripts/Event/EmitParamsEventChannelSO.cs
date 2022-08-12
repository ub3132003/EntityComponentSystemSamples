using UnityEngine;
using UnityEngine.Events;
#if USE_ODIN
using Sirenix.OdinInspector;
#endif
/// <summary>
/// This class is used for Events that have no arguments (Example: Exit game event)
/// </summary>

public class EmitParamsEventChannelSO : DescriptionBaseSO
{
    [SerializeField]
    public UnityAction<ParticleSystem.EmitParams> OnEventRaised;
#if USE_ODIN
    [Button("RaiseEvent")]
    //public int Play;
#endif
    public void RaiseEvent(ParticleSystem.EmitParams emitParams)
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke(emitParams);
    }
}
