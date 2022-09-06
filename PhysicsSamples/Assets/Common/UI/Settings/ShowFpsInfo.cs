using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
public class ShowFpsInfo : MonoBehaviour
{
    public float showTime = 1f;
    public Text tvFpsInfo;

    private int count = 0;
    private float deltaTime = 0f;

    // Update is called once per frame
    void Update()
    {
        count++;
        deltaTime += Time.deltaTime;
        if (deltaTime >= showTime)
        {
            float fps = count / deltaTime;
            float milliSecond = deltaTime * 1000 / count;
            //string strFpsInfo = string.Format(" 当前每帧渲染间隔：{0:0.0} ms ({1:0.} 帧每秒)", milliSecond, fps);
            string strFpsInfo = $"{fps:#}";
            tvFpsInfo.text = strFpsInfo;
            count = 0;
            deltaTime = 0f;
        }
    }
}
