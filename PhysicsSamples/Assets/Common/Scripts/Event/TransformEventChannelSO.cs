using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class is used for Events that have one int argument.
/// Example: An Achievement unlock event, where the int is the Achievement ID.
/// </summary>

public class TransformEventChannelSO : DescriptionBaseSO
{
	public UnityAction<Transform> OnEventRaised;
	
	public void RaiseEvent(Transform value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
