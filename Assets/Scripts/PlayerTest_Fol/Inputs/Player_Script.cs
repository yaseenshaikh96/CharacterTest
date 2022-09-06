using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Script : MonoBehaviour
{
    [SerializeField] private PlayerController playerController;
    [SerializeField] private PlayerAnimator playerAnimator;

    void Start()
    {
        playerController.enabled = true;
        playerAnimator.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
