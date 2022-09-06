using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTest_Script : MonoBehaviour
{
    
    GameData_Script gameData;
    [SerializeField] private InputManager_Script inputManager_Script;
    [SerializeField] private Player_Script player_Script;
    //--------------------------//

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.PlayerTest_Scene.ToString()));
        gameData = GameData_Script.instance;

        inputManager_Script.enabled = true;
        player_Script.enabled = true;
    }

}
