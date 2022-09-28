using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class Camera_Script : MonoBehaviour
{    
    [SerializeField] Camera cameraMain;
    [SerializeField] AudioListener audioListener;
    [SerializeField] CinemachineFreeLook cinemachineFreeLook;
    [SerializeField] CinemachineCollider cinemachineCollider;
    [SerializeField] CinemachineBrain cinemachineBrain;

    void Start()
    {
        cameraMain.enabled = true;
        audioListener.enabled = true;
        cinemachineBrain.enabled = true;
        cinemachineFreeLook.enabled = true;
        cinemachineCollider.enabled = true;
    }

}
