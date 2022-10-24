using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    public static bool isUpdated = false;
    public static GameObject[] sTreePrefabs;
    //------------------------------------------------------------------------------------------//
    Vector3 mWorldPos;
    public GameObject mTreeGO, mParentGO;
    int mTreeIndex;

    //------------------------------------------------------------------------------------------//
    public static void Init(params GameObject[] treePrefabs)
    {
        sTreePrefabs = treePrefabs;
        isUpdated = true;
    }
    public Tree(Vector3 worldPos, GameObject parentGO)
    {
        mWorldPos = worldPos;
        mParentGO = parentGO;

        // mTreeIndex = Random.Range(0, sTreePrefabs.Length);
        mTreeIndex = 0;

        // if(sTreePrefabs.Length != 0)
        mTreeGO = UnityEngine.GameObject.Instantiate(sTreePrefabs[mTreeIndex], mWorldPos, Quaternion.identity);
        // else
        // {
        //     mTreeGO = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
        //     mTreeGO.transform.localPosition = mWorldPos;
        // }
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