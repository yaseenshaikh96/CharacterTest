using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TreeManager : MonoBehaviour
{
    [SerializeField] bool update = false;
    [SerializeField] LayerMask groundLayer;
    [SerializeField] GameObject[] treePrefabs;
    void Start()
    {
        Tree.Init(groundLayer, treePrefabs);
    }
#if UNITY_EDITOR
    void Update()
    {
        if(update)
        {
            update = false;
            Tree.Init(groundLayer, treePrefabs);
        }
    }
#endif
}
