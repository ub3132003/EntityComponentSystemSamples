using System.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.Events;

 
public class EntityChannelSO : DescriptionBaseSO
{
    public event UnityAction<Entity> OnEventRaised;

    public void RaiseEvent(Entity value)
    {
        if (OnEventRaised != null)
            OnEventRaised.Invoke(value);
    }

}
