using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Game_01_Script : MonoBehaviour
{
    GameData_Script gameData;
    //------------------------------------------//
    [SerializeField] private GameObject level_01Prefab;
    [SerializeField] private GameObject playerPrefab;
    private GameObject playerGO;
    private GameObject level_01;

    //--------------------------------------//

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.Game_01_Scene.ToString()));
        gameData = GameData_Script.instance;

        Instantiate(level_01Prefab, Vector3.zero, Quaternion.identity);
        Instantiate(playerPrefab, Vector3.up, Quaternion.identity);

    }

    void Update()
    {
        
    }
}
