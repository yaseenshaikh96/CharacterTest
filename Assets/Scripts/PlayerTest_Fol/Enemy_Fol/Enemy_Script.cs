using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_Script : MonoBehaviour
{
    [SerializeField] Enemy_AI enemy_AI;
    [SerializeField] CharacterController enemyController;
    void Start()
    {
        enemyController.enabled = true;
        enemy_AI.enabled = true;
    }
}
