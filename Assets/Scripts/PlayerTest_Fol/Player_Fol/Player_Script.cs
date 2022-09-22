using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Script : MonoBehaviour
{

    [SerializeField] private PlayerMovement playerMovement;
    [SerializeField] private CameraContoller cameraContoller;

    void Start()
    {
        playerMovement.enabled = true;        
        cameraContoller.enabled = true;
    }

}
