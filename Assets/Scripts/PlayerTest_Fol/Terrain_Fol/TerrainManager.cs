using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TerrainManager : MonoBehaviour
{
    [SerializeField] private TerrainDynamicLoad terrainDynamicLoad;
    [SerializeField] private GameObject playerGO;
    [SerializeField] private GameObject TestParentGO;
    [SerializeField, Range(2, 50)] private int pointsPerChunk;
    [SerializeField, Range(5, 100)] private float chunkSize; // in world pos
    [SerializeField, Range(5, 100)] private float heightMultiplier;
    [SerializeField] private bool isSmoothMesh;
    [SerializeField] private Material mesMaterial;
    [SerializeField] private NoiseData noiseData;
    [SerializeField] private AnimationCurve heightCurve;


    void Start()
    {
        TestParentGO.SetActive(false);

        Chunk.Init(
            playerGO, this.gameObject,
            pointsPerChunk, chunkSize, isSmoothMesh, mesMaterial,
            noiseData, heightMultiplier, heightCurve
        );
        terrainDynamicLoad.enabled = true;
    }
    //------------------------------------------------------------------------------------------------//

    /*
    #if UNITY_EDITOR
        Chunk[,] chunksForEditor;
        void Update() // only for editor
        {
            if (clear)
            {
                foreach (Transform child in TestParentGO.transform)
                {
                    DestroyImmediate(child.gameObject);
                }
                clear = false;
            }
            if (update)
            {
                Debug.Log("Updated");
                update = false;

                Chunk.Init
                (
                    playerGO, TestParentGO,
                    pointsPerChunk, chunkSize, isSmoothMesh, mesMaterial,
                    noiseData,
                    heightMultiplier, heightCurve
                );

                MakeBigSquareEditorVer();
            }
        }

        void MakeBigSquareEditorVer()
        {

            if (chunksForEditor != null)
            {
                for (int x = 0; x < chunksForEditor.GetLength(0); x++)
                {
                    for (int z = 0; z < chunksForEditor.GetLength(1); z++)
                    {
                        DestroyImmediate(chunksForEditor[x, z].meshGO);
                    }
                }
            }
            chunksForEditor = new Chunk[10, 10];
            for (int x = 0; x < chunksForEditor.GetLength(0); x++)
            {
                for (int z = 0; z < chunksForEditor.GetLength(1); z++)
                {
                    chunksForEditor[x, z] = new Chunk(new Vector3(x, 0, z) ;
                    chunksForEditor[x, z].Generate();
                    chunksForEditor[x, z].MakeGameObject();
                    chunksForEditor[x, z].AddCollider();
                }
            }
        }
    #endif
    */
}
