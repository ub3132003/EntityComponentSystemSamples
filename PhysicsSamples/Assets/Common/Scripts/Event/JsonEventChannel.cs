using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class JsonEventChannel : DescriptionBaseSO
{
    [SerializeField]
    public UnityAction<string> OnEventRaised;
 
    public void RaiseEvent(string val)
    {
        OnEventRaised?.Invoke(val);
    }
}
