using System.Collections;
using System.Collections.Generic;
using Unity.Collections.LowLevel.Unsafe;

using UnityEngine;
//指示轨迹方向
public class LaunchIndicatorLine : MonoBehaviour
{
    [SerializeField]
    LineRenderer line;

    public void SetPosition(int index , Vector3 point) => line.SetPosition(index, point);

    private void OnEnable()
    {
    }

    public void SetLineIndection(Vector3 endPosition)
    {
        SetPosition(0, transform.position);
        SetPosition(1, endPosition);
    }

    public void SetLineIndection(Vector3 start, Vector3 end)
    {
        SetPosition(0, start);
        SetPosition(1, end);
    }
}
