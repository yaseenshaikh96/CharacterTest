using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager_Script : MonoBehaviour
{
    [SerializeField] private GameObject levelPrefab;
    [SerializeField] private LayerMask groundLayer;
    private GameObject levelGO;

    void Start()
    {
        levelGO = Instantiate(levelPrefab, Vector3.zero, Quaternion.identity);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
