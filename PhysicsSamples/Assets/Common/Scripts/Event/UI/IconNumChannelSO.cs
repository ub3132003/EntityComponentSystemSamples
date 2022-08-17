using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class IconNumChannelSO : DescriptionBaseSO
{
    public UnityAction<Sprite , int> OnEventRaised;

    public void RaiseEvent(Sprite icon, int amount)
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke(icon, amount);
    }
}
