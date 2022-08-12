using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// This class is used for Events that have one int argument.
/// Example: An Achievement unlock event, where the int is the Achievement ID.
/// </summary>


public class Vector2IntEventChannelSO : DescriptionBaseSO
{
	public UnityAction<Vector2Int> OnEventRaised;
	
	public void RaiseEvent(Vector2Int value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
