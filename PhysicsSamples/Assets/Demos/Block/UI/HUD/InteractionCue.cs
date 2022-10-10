using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using DG.Tweening;

public class InteractionCue : MonoBehaviour
{
    [SerializeField] RectTransform Parent;
    [SerializeField] RectTransform tipCueinteraction;
    [SerializeField] Vector2 offset;
    [SerializeField] IntGameObjectEventChannelSO  interactionEvent;
    [SerializeField] Ease animCurve;
    private void OnEnable()
    {
        interactionEvent.OnEventRaised += OnReadyBallchange;
    }

    private void OnDisable()
    {
        interactionEvent.OnEventRaised -= OnReadyBallchange;
    }

    void Update()
    {
        if (potentialInteraction.Count > 0)
        {
            if (Input.GetKeyUp(KeyCode.E))//只执行一次;
            {
                foreach (var item in potentialInteraction)
                {
                    item.GetComponent<IInteraction>().PlayerInteraction(item.transform);
                }
                //potentialInteraction.Clear();
                //ExpandCueUI(false);
                tipCueinteraction.transform.DOScale(1f, 0.2f).From(0.8f).SetEase(animCurve);
            }
        }
    }

    public List<GameObject> potentialInteraction = new List<GameObject>();
    private void OnReadyBallchange(int stat, GameObject target)
    {
        var action = target.GetComponent<IInteraction>();
        if (action == null)
        {
            Debug.LogWarning("No Interaction Implementation ");
            return;
        }
        var interactionStat = Convert.ToBoolean(stat);
        if (interactionStat)
        {
            potentialInteraction.Add(target);
        }
        else
        {
            potentialInteraction.Remove(target);
        }


        ExpandCueUI(interactionStat);
        UIFowllow(Parent, tipCueinteraction, target.transform, offset);
    }

    public void ExpandCueUI(bool opt)
    {
        tipCueinteraction.gameObject.SetActive(opt);
    }

    // 遮挡问题, 视野的显示问题.
    public static void UIFowllow(RectTransform parent, RectTransform rectTrans, Transform target, Vector2 offset)
    {
        var screenPoint = RectTransformUtility.WorldToScreenPoint(Camera.main, target.position);
        if (RectTransformUtility.ScreenPointToLocalPointInRectangle(parent, screenPoint, null, out var vector2))
        {
            rectTrans.localPosition = vector2 + offset;
        }
    }
}
