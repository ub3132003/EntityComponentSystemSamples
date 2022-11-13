using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

public class UIBallToggle : MonoBehaviour
{
    [SerializeField] Toggle toggle;
    RectTransform rectTransform;
    bool oldToggleValue;

    public Vector2 moveOnselect = new Vector2(10, 0);
    public float SclaeOnselect = 1.1f;

    [SerializeField] Image Icon;

    UnityAction<bool, int> onChangeToggle;
    int option = 0;
    //bool isInit = false;
    public void Init(int option)
    {
        this.option = option;
        //isInit = true;
    }

    private void Start()
    {
        toggle.onValueChanged.AddListener(onToggleChange);
        rectTransform = transform as RectTransform;
        oldToggleValue = toggle.isOn;
    }

    public void SetToggleOn()
    {
        if (toggle.interactable == false) return;
        toggle.isOn = true;
    }

    public void SetBallUI(Sprite sprite)
    {
        if (sprite == null)
        {
            Icon.enabled = false;
            toggle.interactable = false;
        }
        else
        {
            toggle.interactable = true;
            Icon.enabled = true;
        }
        Icon.sprite = sprite;
    }

    public void SetToggleEvent(UnityAction<bool, int> unityAction)
    {
        onChangeToggle = unityAction;
    }

    private void onToggleChange(bool opt)
    {
        if (oldToggleValue == opt)
        {
            return;
        }
        else
        {
            oldToggleValue = opt;
        }

        SelectView(opt);
        onChangeToggle.Invoke(opt, option);
    }

    void SelectView(bool opt)
    {
        if (opt)
        {
            transform.localScale = Vector3.one * SclaeOnselect;
            rectTransform.anchoredPosition += moveOnselect;
        }
        else
        {
            transform.localScale = Vector3.one;
            rectTransform.anchoredPosition += -moveOnselect;
        }
    }
}
