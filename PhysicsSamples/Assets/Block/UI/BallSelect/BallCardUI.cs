using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using Unity.Mathematics;
using DG.Tweening;

public class BallCardUI : MonoBehaviour, IPointerClickHandler, ICardUI
{
    [SerializeField] Button CardButton;
    [SerializeField] Toggle CardToggle;
    //整卡背景
    [SerializeField] Image BG;
    [SerializeField] Image Icon;
    //图标背景,职业
    [SerializeField] Image IconBGI;
    //稀有度
    [SerializeField] Image RaceImage;


    [SerializeField] TextMeshProUGUI numValue;
    [SerializeField] CanvasGroup disableCanvas;

    public UnityAction<bool> OnValueChanged;

    static int currentTargetPositionId = 0;
    List<Transform> TargetPositionList => BallAbillityManager.Instance.TargetPositionList;
    public void Start()
    {
        CardToggle.onValueChanged.AddListener(OnValueChanged);
        CardToggle.onValueChanged.AddListener(SetSelected);

        currentTargetPositionId = 0;
    }

    public void SetCard(Sprite icon, int num)
    {
        Icon.sprite = icon;
        numValue.text = num.ToString();
    }

    GameObject tempAnimaObj;
    //显示灰色+动画
    public void SetSelected(bool opt)
    {
        if (currentTargetPositionId > 4 && opt == true)//超过上限的卡
        {
            CardToggle.isOn = false;
            return;
        }
    }

    public void SetNull(bool opt)
    {
        Icon.enabled = opt;
        RaceImage.enabled = opt;
    }

    public void SetBackGroudColor(Color color)
    {
        BG.color = color;
    }

    /// <summary>
    /// 点击卡牌时执行
    /// </summary>
    public UnityAction SubmitAction { get; set; }

    public int CardId { get; set; }

    private void Awake()
    {
        //CardButton.onClick.AddListener(OnClick);
    }

    //显示灰色+动画 在valuechange 之前调用
    public void OnPointerClick(PointerEventData eventData)
    {
        bool opt = !CardToggle.isOn; //点击后的状态
        if (currentTargetPositionId > 3 && opt == true)
        {
            return;
        }


        var goPos = opt ? TargetPositionList[currentTargetPositionId].position : transform.position;
        var speed = 0.1f;
        //选择是移动到选择栏
        if (opt == true)
        {
            BallAbillityManager.Instance.BallSelectedContent.GetComponent<HorizontalLayoutGroup>().enabled = false;

            var sbling = transform.GetSiblingIndex();
            disableCanvas.alpha = 0.6f;
            tempAnimaObj = Instantiate(this.gameObject, transform.parent); disableCanvas.alpha = 1f;
            tempAnimaObj.transform.SetSiblingIndex(sbling);
            Destroy(tempAnimaObj.GetComponent<BallCardUI>());
            //TempAnimaObj.GetComponent<RectTransform>().sizeDelta = GetComponent<RectTransform>().sizeDelta;
            //TempAnimaObj.transform.position = transform.position;
            transform.SetParent(BallAbillityManager.Instance.BallSelectedContent);

            var tween = transform.DOMove(goPos, speed);
        }
        else
        {
            BallAbillityManager.Instance.BallSelectedContent.GetComponent<HorizontalLayoutGroup>().enabled = true;

            var sibling = tempAnimaObj.transform.GetSiblingIndex();
            var tween = transform.DOMove(tempAnimaObj.transform.position, speed);
            tween.OnComplete(() =>
            {
                transform.SetParent(tempAnimaObj.transform.parent);
                transform.SetSiblingIndex(sibling);
                Destroy(tempAnimaObj);
            });
        }

        currentTargetPositionId += opt.ToDiff();
    }

    public void SetCard(string title, string detail, Sprite icon, int rank)
    {
        throw new System.NotImplementedException();
    }

    public void SetIntactionable(bool opt)
    {
        throw new System.NotImplementedException();
    }
}
