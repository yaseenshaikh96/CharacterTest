using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu_Script : MonoBehaviour
{
    private GameData_Script gameData;

    //-----------------------------------------------------------//

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.MainMenu_Scene.ToString()));
        gameData = GameData_Script.instance;
        this.GetComponent<MainMenu_HandleUI>().enabled = true;
    }
    void Update() 
    {

    }
}