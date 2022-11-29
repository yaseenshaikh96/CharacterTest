using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grass
{

    static GameObject[] sGrassPrefabs;
    static GameObject[] sGrassLongPrefabs;
    static GameObject[] sGrassShortPrefabs;
    static LayerMask sGroundLayer;
    //-----------------------------------------------------//

    Vector3 mWorldPos;
    public GameObject mGrassGO;
    GameObject mParentGO;
    int mGrassIndex;

    //-----------------------------------------------------//

    public static void Init(LayerMask groundLayer, GameObject[] grassPrefabs, GameObject[] grassLongPrefabs, GameObject[] grassShortPrefabs)
    {
        sGroundLayer = groundLayer;
        sGrassPrefabs = grassPrefabs;
        sGrassLongPrefabs = grassLongPrefabs;
        sGrassShortPrefabs = grassShortPrefabs;
    }

    public Grass(Vector3 worldPos, GameObject parentGO)
    {
        mWorldPos = worldPos;
        mParentGO = parentGO;
        mGrassIndex = Random.Range(0, sGrassPrefabs.Length);
        // mTreeIndex = 0;

        // if(sTreePrefabs.Length != 0)
        float yRotDeg = Random.Range(0, 360);
        float xRotDeg = Random.Range(0, 10);
        float zRotDeg = Random.Range(0, 10);
        Quaternion treeRot = Quaternion.Euler(xRotDeg, yRotDeg, zRotDeg);
        mGrassGO = UnityEngine.GameObject.Instantiate(sGrassPrefabs[mGrassIndex], mWorldPos, treeRot);
        mGrassGO.layer = sGroundLayer.value >> 5;
        mGrassGO.transform.parent = mParentGO.transform;
    }
    public void Delete()
    {
        UnityEngine.GameObject.Destroy(mGrassGO);
    }
}
