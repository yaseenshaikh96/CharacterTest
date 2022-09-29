using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager_Script : MonoBehaviour
{
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private TerrainDynamicLoad terrainDynamicLoad;
    [SerializeField] private TreeManager treeManager;

    void Start()
    {
        chunkManager.enabled = true;
        terrainDynamicLoad.enabled = true;
        treeManager.enabled =true;
    }

}
