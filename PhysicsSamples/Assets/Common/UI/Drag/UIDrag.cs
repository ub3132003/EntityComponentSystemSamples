using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using RectTransformExtensions;
using UnityEngine.Events;

public class UIDrag : MonoBehaviour, IDragHandler, IPointerDownHandler, IEndDragHandler
{
    /// <summary>
    /// true 会校验是否在指定区域内，进入离开指定区域是调用回调。
    /// </summary>
    public bool useArea;
    /// <summary>
    /// 启用后禁止在所属区域内
    /// </summary>
    public bool useBanArea;
    public List<RectTransform> validArea;


    private Vector2 offsetPos;  //临时记录点击点与UI的相对位置
    private Vector2 startPos;
    //ui 对象跟随拖动
    private bool canMoveSelf = true;
    public void OnDrag(PointerEventData eventData)
    {
        if (canMoveSelf)
        {
            transform.position = eventData.position - offsetPos;
        }


        if (useArea)
        {
            isInValidArea = IsInArea(validArea);
            if (IsInValidArea)
            {
                OnEnterValidArea.Invoke();
            }
            else
            {
                OnExitValidArea.Invoke();
            }
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        offsetPos = eventData.position - (Vector2)transform.position;
        startPos = transform.position;
    }

    public bool IsInArea(List<RectTransform> AreaList)
    {
        bool isInValidArea = false;
        foreach (var item in AreaList)
        {
            if (item.Contains(transform as RectTransform))
            {
                isInValidArea = true;
                break;
            }
        }
        return isInValidArea;
    }

    public UnityEvent OnEnterValidArea;
    public UnityEvent OnExitValidArea;
    private bool isInValidArea = false;

    public bool IsInValidArea { get => isInValidArea; }
    public bool CanMoveSelf { get => canMoveSelf; set => canMoveSelf = value; }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (IsInValidArea == false && useArea)
        {
            transform.position = startPos;
        }

        //if (useBanArea)
        //{
        //    bool isInBanArea = false;
        //    foreach (var item in validArea)
        //    {

        //        if (item.Contains(transform as RectTransform))
        //        {
        //            isInBanArea = true;
        //            break;
        //        }
        //    }
        //    if (isInBanArea)
        //    {
        //        Destroy(this);
        //    }
        //}
    }
}
