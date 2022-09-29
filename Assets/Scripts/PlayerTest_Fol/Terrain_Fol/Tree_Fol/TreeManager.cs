using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreeManager : MonoBehaviour
{
    [SerializeField] GameObject[] treePrefabs;
    void Start()
    {
        Tree.Init(treePrefabs);
    }

}
