using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class PlayerTest_Script : MonoBehaviour
{
    enum RunTypeE
    {
        terrain, enemy, empty, animation
    }


    GameData_Script gameData;

    [SerializeField] private RunTypeE runTypeE; 
    [SerializeField] private InputManager_Script inputManager_Script;
    [SerializeField] private Player_Script player_Script;
    [SerializeField] private Enemy_Script enemy_Script;
    [SerializeField] private Camera_Script camera_Script;
    [SerializeField] private TerrainManager_Script terrainManager_Script;
    //--------------------------//

    void Start()
    {
        QualitySettings.vSyncCount = 1;
        Application.targetFrameRate = 30;
  
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.PlayerTest_Scene.ToString()));
        gameData = GameData_Script.instance;

        switch(runTypeE)
        {
            case RunTypeE.terrain:
                inputManager_Script.enabled = true;
                player_Script.enabled = true;
                camera_Script.enabled = true;
                terrainManager_Script.enabled = true;
                enemy_Script.enabled = false;
                break;
            case RunTypeE.enemy:
                enemy_Script.enabled = true;
                inputManager_Script.enabled = false;
                player_Script.enabled = false;
                camera_Script.enabled = false;
                terrainManager_Script.enabled = false; 
                break;
            case RunTypeE.empty:
                inputManager_Script.enabled = false;
                player_Script.enabled = false;
                camera_Script.enabled = false;
                terrainManager_Script.enabled = false;
                enemy_Script.enabled = false;
                break;
            case RunTypeE.animation:
                inputManager_Script.enabled = true;
                player_Script.enabled = true;
                camera_Script.enabled = true;
                terrainManager_Script.enabled = false;
                enemy_Script.enabled = false;
                break;
        }
    }

}
