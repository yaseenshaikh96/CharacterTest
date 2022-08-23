using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test2 : MonoBehaviour
{
    GameData_Script gameData;
    int i=0;
    // Start is called before the first frame update
    void Start()
    {
        gameData = GameData_Script.instance;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.TestScene2.ToString()));
    }
    void Update() {
        i++;
        if(i==200) {
            gameData.ChangeScene(SceneNameEnum.MainMenu_Scene);
        }
    }

}