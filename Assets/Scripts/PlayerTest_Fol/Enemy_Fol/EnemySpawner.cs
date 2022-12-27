using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] public GameObject playerGO;
    [SerializeField] public LayerMask groundLayer;
    [SerializeField] public float unloadDistance;
    [SerializeField] public float chasingDistance;
    [SerializeField] public float attackDistance;

    public Vector2 playerWorldPos2D {get; private set;}
    public Vector3 playerWorldPos {get; private set;}

    void Start()
    {
        // spawn enemies
        playerWorldPos = new Vector3();
        playerWorldPos2D = new Vector2();
        Enemy_AI.enemySpawner = this;


        Instantiate(EnemyPrefab, new Vector3(0, 0, -50), Quaternion.identity);
    }

    void Update()
    {
        // update player pos
        playerWorldPos = playerGO.transform.position;
        playerWorldPos2D = new Vector2(playerGO.transform.position.x, playerGO.transform.position.z);
    }
}
