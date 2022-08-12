using UnityEngine;
using UnityEngine.Events;
/// <summary>
/// 输入0-1 的百分比,或者 0-最大值的数
/// </summary>
public class SilderEventChannel : DescriptionBaseSO
{
	public UnityAction<float> OnEventRaised;
    public float MaxValue=1;
	public void RaiseEvent( float value)
	{
		if (OnEventRaised != null)
			OnEventRaised.Invoke(value);
	}
}
