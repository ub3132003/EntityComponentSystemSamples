using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class UpdateSkillSelectUI : MonoBehaviour
{
    [SerializeField] Button CardButton;
    [SerializeField] Image Icon;
    [SerializeField] TextMeshProUGUI Title;
    [SerializeField] TextMeshProUGUI description;
    void Start()
    {
    }

    // Update is called once per frame
    void Update()
    {
    }

    public void SetCard(ThingSO cardSO)
    {
        description.text = cardSO.Description.GetLocalizedString();
    }

    public void Submit()
    {
    }
}
