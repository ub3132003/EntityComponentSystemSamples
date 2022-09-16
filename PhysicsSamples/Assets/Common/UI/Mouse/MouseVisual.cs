using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseVisual : MonoBehaviour
{
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
        MouseFollow();
    }

    void MouseFollow()
    {
        Vector3 pos = Camera.main.WorldToScreenPoint(transform.position);
        //让鼠标的屏幕坐标与对象坐标一致
        Vector3 mousePos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, pos.z);
        //转换成世界坐标
        transform.position = Camera.main.ScreenToWorldPoint(mousePos);
    }
}
