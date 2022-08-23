using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Test1 : MonoBehaviour
{
    int i=0;
    GameData_Script gameData;
    // Start is called before the first frame update
    void Start()
    {
        gameData = GameData_Script.instance;
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.TestScene1.ToString()));
    }

    void Update() {
        i++;
        if(i==200) {
            gameData.ChangeScene(SceneNameEnum.TestScene2);
        }
    }
}