using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EnemySpawner_Script : MonoBehaviour
{
    GameData_Script gameData;

    //--------------------------//

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.EnemySpawner_Scene.ToString()));
        gameData = GameData_Script.instance;
    }

    void Update()
    {
        
    }
}
