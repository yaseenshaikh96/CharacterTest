using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class SpawnableManager : MonoBehaviour
{
    [SerializeField] bool update = false;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject[] treePrefabs;
    [SerializeField] GameObject[] grassPrefabs;
    [SerializeField] GameObject[] grassShortPrefabs;
    [SerializeField] GameObject[] grassLongPrefabs;
    void Start()
    {
        Tree.Init(groundLayer, treePrefabs);
        Grass.Init(groundLayer, grassPrefabs, grassLongPrefabs, grassShortPrefabs);
    }
#if UNITY_EDITOR
    void Update()
    {
        if (update)
        {
            update = false;
            Tree.Init(groundLayer, treePrefabs);
            Grass.Init(groundLayer, grassPrefabs, grassLongPrefabs, grassShortPrefabs);
        }
    }
#endif
}
