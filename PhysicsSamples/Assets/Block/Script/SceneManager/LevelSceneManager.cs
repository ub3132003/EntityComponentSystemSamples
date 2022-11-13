using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Scenes;
public class LevelSceneManager : Singleton<LevelSceneManager>
{
    public List<SubScene> subScenes;

    //boradcast on
    [SerializeField] IntEventChannelSO LevelCompleteEvent;
    //lisenting in
    public IntEventChannelSO ChangeLevelSceneEvent;
    [SerializeField] IntEventChannelSO BoxNumEvent;
    [SerializeField] IntEventChannelSO BallNumEvent;
    private SceneSystem sceneSystem;
    private int currentSceneIdx;
    private void Start()
    {
        sceneSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SceneSystem>();
    }

    public void OnEnable()
    {
        BoxNumEvent.OnEventRaised += OnBoxNumChange;
        ChangeLevelSceneEvent.OnEventRaised += LoadLevelScene;
    }

    public void OnDisable()
    {
        BoxNumEvent.OnEventRaised -= OnBoxNumChange;
        ChangeLevelSceneEvent.OnEventRaised -= LoadLevelScene;
    }

    SubScene currentScene;
    public void LoadLevelScene(int sceneId)
    {
        if (currentScene != null)
        {
            sceneSystem.UnloadScene(currentScene.SceneGUID);
        }
        currentScene = subScenes[sceneId];
        sceneSystem.LoadSceneAsync(currentScene.SceneGUID);
        currentSceneIdx = sceneId;
    }

    void OnBoxNumChange(int n)
    {
        if (n == 0)
        {
            Debug.Log("过关了");
        }
    }
}
