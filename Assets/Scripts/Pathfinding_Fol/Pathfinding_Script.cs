using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Pathfinding_Script : MonoBehaviour
{
    GameData_Script gameData;
    //--------------------------//
    [SerializeField] private Camera cameraMain;

    void Start()
    {
        SceneManager.SetActiveScene(SceneManager.GetSceneByName(SceneNameEnum.Pathfinding_Scene.ToString()));
        gameData = GameData_Script.instance;

        cameraMain.orthographic = true;
        cameraMain.orthographicSize = 6.5f;
        GetComponent<CreateGrid>().enabled = true;
    }

    void Update()
    {
        
    }
}
