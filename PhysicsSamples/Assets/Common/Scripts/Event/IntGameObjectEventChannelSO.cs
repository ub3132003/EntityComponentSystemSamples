using UnityEngine;
using UnityEngine.Events;

/// <summary>
/// 打开面板 关闭上一个此类型的面板
/// </summary>

public class IntGameObjectEventChannelSO : DescriptionBaseSO
{
	public UnityAction<int ,GameObject > OnEventRaised;
	private void RaiseEvent( int typeKey,GameObject obj)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(typeKey, obj);
	}
}
