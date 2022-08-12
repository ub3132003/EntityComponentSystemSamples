using UnityEngine;
using UnityEngine.Events;

/// <summary>
//放置物品等 网格,和物品对象
/// </summary>

public class Vector2ObjEventChannelSO : DescriptionBaseSO
{
	public UnityAction<Vector2,GameObject> OnEventRaised;
	
	public void RaiseEvent(Vector2 value,GameObject obj)
	{
			OnEventRaised?.Invoke(value,obj);
	}
}
