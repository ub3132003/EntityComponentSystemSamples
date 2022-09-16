using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Entities;
using Unity.Scenes;
public class LevelSceneManager : Singleton<LevelSceneManager>
{
    public List<SubScene> subScenes;
    public IntEventChannelSO ChangeLevelSceneEvent;
    private SceneSystem sceneSystem;
    private void Start()
    {
        sceneSystem = World.DefaultGameObjectInjectionWorld.GetOrCreateSystem<SceneSystem>();
        ChangeLevelSceneEvent.OnEventRaised = LoadLevelScene;
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
    }
}
