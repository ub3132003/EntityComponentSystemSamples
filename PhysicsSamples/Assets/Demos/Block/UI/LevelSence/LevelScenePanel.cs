using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LevelScenePanel : MonoBehaviour
{
    [SerializeField] GameObject gridSelectContent;
    [SerializeField] List<Button> levelButtons;
    [SerializeField] IntEventChannelSO levelChangeEvent;

    [SerializeField] GameObject sigleLevelButton;
    int currentLevelIdx;

    public int CurrentLevelIdx { get => currentLevelIdx; }

    void Start()
    {
        levelButtons = new List<Button>(gridSelectContent.GetComponentsInChildren<Button>());
        for (int i = 0; i < levelButtons.Count; i++)
        {
            var button = levelButtons[i];
            var id = i;
            button.onClick.AddListener(() =>
            {
                levelChangeEvent.RaiseEvent(id);
                currentLevelIdx = id;
            });
        }
    }

    public void NextScene()
    {
        currentLevelIdx++;
        levelChangeEvent.RaiseEvent(currentLevelIdx);
    }

    public void SetSigleLevel()
    {
        var button = sigleLevelButton.GetComponent<Button>();
        var text = sigleLevelButton.GetComponentInChildren<TextMeshProUGUI>();
        text.text = "Level 1-1";
    }
}