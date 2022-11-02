using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Tree
{
    public static bool isUpdated = false;
    static GameObject[] sTreePrefabs;
    static LayerMask sGroundLayer;
    //------------------------------------------------------------------------------------------//
    Vector3 mWorldPos;
    public GameObject mTreeGO, mParentGO;
    Collider mCollider;
    int mTreeIndex;

    //------------------------------------------------------------------------------------------//
    public static void Init(LayerMask groundLayer, params GameObject[] treePrefabs)
    {
        sGroundLayer = groundLayer;
        sTreePrefabs = treePrefabs;
        isUpdated = true;
    }
    public Tree(Vector3 worldPos, GameObject parentGO)
    {
        mWorldPos = worldPos;
        mParentGO = parentGO;
        mTreeIndex = Random.Range(0, sTreePrefabs.Length);
        // mTreeIndex = 0;

        // if(sTreePrefabs.Length != 0)
        float yRotDeg = Random.Range(0, 360);
        Quaternion treeRot = Quaternion.Euler(0, yRotDeg, 0);
        mTreeGO = UnityEngine.GameObject.Instantiate(sTreePrefabs[mTreeIndex], mWorldPos, treeRot);
        mTreeGO.layer = sGroundLayer.value >> 5;
        mCollider = mTreeGO.GetComponent<Collider>();
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
    public void AddCollider()
    {
        mCollider.enabled = true;
    }
    public void RemoveCollider()
    {
        mCollider.enabled = false;
    }
}