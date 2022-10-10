using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FaceToCamera : MonoBehaviour
{
    public bool reverse = false;
    void Update()
    {
        //transform.LookAt(Camera.main.transform.position); // xyz ����ǰ
        var m_cam_main_Transform = Camera.main.transform;
        var m_Cam_Positon = m_cam_main_Transform.position;
        var m_Positon = this.transform.position;
        Vector3 Dire = m_Cam_Positon - m_Positon;
        Dire = Dire.normalized;
        if (reverse)
        {
            Dire = -Dire;
        }


        this.transform.forward = Dire; // �����ķ���ָ�������
    }
}
