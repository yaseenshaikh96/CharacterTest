using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player_Script : MonoBehaviour
{

    [SerializeField] private PlayerMovement playerMovement;

    void Start()
    {
        playerMovement.enabled = true;        
    }

}
