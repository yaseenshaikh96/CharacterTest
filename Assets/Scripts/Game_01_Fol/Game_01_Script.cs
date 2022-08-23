using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_01_Script : MonoBehaviour
{
    GameData_Script gameData;

    //--------------------------------------//

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.Game_01_Scene.ToString()));
        gameData = GameData_Script.instance;
    }

    void Update()
    {
        
    }
}
