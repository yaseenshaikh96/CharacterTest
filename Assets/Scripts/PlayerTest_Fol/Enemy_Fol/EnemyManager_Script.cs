using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyManager_Script : MonoBehaviour
{
    [SerializeField] private EnemySpawner enemySpawner;

    void Start()
    {
        enemySpawner.enabled = true;
    }
}