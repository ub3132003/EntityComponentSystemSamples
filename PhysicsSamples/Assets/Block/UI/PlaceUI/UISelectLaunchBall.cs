using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UISelectLaunchBall : MonoBehaviour, Ipanel
{
    public TMP_Dropdown dropdown;


    /// <summary>
    /// 设置选项
    /// </summary>
    public void InitSelectOption(List<ThingSO> ballSoList)
    {
        List<TMP_Dropdown.OptionData> optionDatas = new List<TMP_Dropdown.OptionData>();
        foreach (var item in ballSoList)
        {
            optionDatas.Add(
                new TMP_Dropdown.OptionData
                {
                    image = item.PreviewImage,
                    text = item.Name.GetLocalizedString(),
                });
        }
        dropdown.AddOptions(optionDatas);
    }

    public void Open()
    {
        gameObject.SetActive(true);
    }

    public void Close()
    {
        gameObject.SetActive(false);
    }
}
