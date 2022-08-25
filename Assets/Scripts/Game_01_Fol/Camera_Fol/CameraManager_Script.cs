using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraManager_Script : MonoBehaviour
{
    [SerializeField] private Camera cameraMain;
    private Player player;
    void Start()
    {
        player = PlayerManager_Script.player;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
