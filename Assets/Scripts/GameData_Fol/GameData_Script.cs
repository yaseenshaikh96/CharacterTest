using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameData_Script : MonoBehaviour
{
    //---------------------------------------//
    public static GameData_Script instance;
    //---------------------------------------//
    SceneNameEnum currentScene;
    SceneNameEnum currentSceneNotThreadSafe;
    public bool IsChangingScene {get; private set;}

    //---------------------------------------//

    void Awake()
    {
        IsChangingScene = false;
        instance = this;
        currentScene = SceneNameEnum.GameData_Scene;
        currentSceneNotThreadSafe = SceneNameEnum.GameData_Scene;
        ChangeScene(SceneNameEnum.MainMenu_Scene);
    }

    //------------------------------------------//

    public void ChangeScene(SceneNameEnum newSceneName)
    {
        currentSceneNotThreadSafe = newSceneName;
        StartCoroutine(ChangeSceneInternal(newSceneName));
    }

    private IEnumerator ChangeSceneInternal(SceneNameEnum newSceneName)
    {
        IsChangingScene = true;
        if (currentScene != SceneNameEnum.GameData_Scene)
        {
            AsyncOperation sceneUnloadAsync = SceneManager.UnloadSceneAsync(currentScene.ToString());
            while (!sceneUnloadAsync.isDone)
            {
                yield return null;
            }
        }
        AsyncOperation sceneLoadAsync = SceneManager.LoadSceneAsync(newSceneName.ToString(), LoadSceneMode.Additive);
        while (!sceneLoadAsync.isDone)
        {
            yield return null;
        }
        currentScene = newSceneName;
        IsChangingScene = false;
    }
}
