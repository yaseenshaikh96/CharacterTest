using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{

    public static GameObject[] sTreePrefabs;
    //------------------------------------------------------------------------------------------//
    Vector3 mWorldPos;
    public GameObject mTreeGO, mParentGO;
    int mTreeIndex;

    //------------------------------------------------------------------------------------------//
    public static void Init(GameObject[] treePrefabs)
    {
        sTreePrefabs = treePrefabs;
    }
    public Tree(Vector3 worldPos, GameObject parentGO)
    {
        mWorldPos = worldPos;
        mParentGO = parentGO;

        mTreeIndex = Random.Range(0, sTreePrefabs.Length);
        
        mTreeGO = UnityEngine.GameObject.Instantiate(sTreePrefabs[mTreeIndex], mWorldPos, Quaternion.identity);
        mTreeGO.transform.parent = mParentGO.transform;
    }
    public void Delete()
    {
        UnityEngine.GameObject.Destroy(mTreeGO);
    }
    ~Tree()
    {
        Delete();
    }
    //------------------------------------------------------------------------------------------//

}