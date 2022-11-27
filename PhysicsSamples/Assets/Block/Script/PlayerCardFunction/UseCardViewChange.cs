using System.Collections;
using System.Collections.Generic;
using UnityEngine;
//拖动卡牌，改变外观预览
public class UseCardViewChange : MonoBehaviour
{
    [SerializeField] GameObject NormalView;//默认卡牌外观
    [SerializeField] GameObject PerView; //拖到场景中的预览
    [SerializeField] GameObject SucceedView; // 找到释放对象时的预览.
    public void SetNormalView()
    {
        NormalView.gameObject.SetActive(true);
        PerView.gameObject.SetActive(false);
        SucceedView.gameObject.SetActive(false);
    }

    public void SetPerView()
    {
        NormalView.gameObject.SetActive(false);
        PerView.gameObject.SetActive(true);
        SucceedView.gameObject.SetActive(false);
    }

    public void SetSucceedView()
    {
        NormalView.gameObject.SetActive(false);
        PerView.gameObject.SetActive(false);
        SucceedView.gameObject.SetActive(true);
    }
}
