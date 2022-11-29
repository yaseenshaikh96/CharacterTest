using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager_Script : MonoBehaviour
{
    [SerializeField] private ChunkManager chunkManager;
    [SerializeField] private TerrainDynamicLoad terrainDynamicLoad;
    [SerializeField] private SpawnableManager spawnableManager;
    [SerializeField] private ChunkColor_Script chunkColor_Script;

    void Start()
    {
        chunkManager.enabled = true;
        terrainDynamicLoad.enabled = true;
        spawnableManager.enabled =true;
        chunkColor_Script.enabled = true;
    }

}
