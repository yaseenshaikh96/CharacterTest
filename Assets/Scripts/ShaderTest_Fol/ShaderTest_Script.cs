using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShaderTest_Script : MonoBehaviour
{
    private GameData_Script gameData;
    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.ShaderTest_Scene.ToString()));
        gameData = GameData_Script.instance;
    }
}
