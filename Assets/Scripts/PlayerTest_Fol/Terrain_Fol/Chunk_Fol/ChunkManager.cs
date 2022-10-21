using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class ChunkManager : MonoBehaviour
{
    [SerializeField] private bool update, clear;
    [SerializeField, Range(4, 30)] private int ChunkLoadCountForEditor;
    [SerializeField] private TerrainDynamicLoad terrainDynamicLoad;
    [SerializeField] private GameObject playerGO;
    [SerializeField] private GameObject TestParentGO;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField, Range(2, 50)] private int pointsPerChunk;
    [SerializeField, Range(5, 100)] private float chunkSize; // in world pos
    [SerializeField, Range(5, 700)] private float heightMultiplier;
    [SerializeField] private MeshType meshType;
    [SerializeField] private Material meshMaterial;
    [SerializeField] private Material waterMaterial;
    [SerializeField] private NoiseData noiseData;
    [SerializeField] private AnimationCurve heightCurve;

    public static List<float> allpoints;

    void Start()
    {
        if (Application.isPlaying)
        {
            TestParentGO.SetActive(false);

            Chunk.Init(
                playerGO, this.gameObject, groundLayer,
                pointsPerChunk, chunkSize, meshType, meshMaterial, waterMaterial,
                noiseData, heightMultiplier, heightCurve
            );
            terrainDynamicLoad.enabled = true;
        }
    }
    //------------------------------------------------------------------------------------------------//


#if UNITY_EDITOR
    Chunk[,] chunksForEditor;
    void Update() // only for editor
    {
        if (clear)
        {
            List<Transform> children = new List<Transform>();
            foreach (Transform child in TestParentGO.transform)
                children.Add(child);
            foreach(Transform child in TestParentGO.transform)
                DestroyImmediate(child.gameObject);
                
            clear = false;
        }
        if (update)
        {
            allpoints = new List<float>();
            Debug.Log("Updated");
            update = false;

            Chunk.Init
            (
                playerGO, TestParentGO, groundLayer,
                pointsPerChunk, chunkSize, meshType, meshMaterial, waterMaterial,
                noiseData,
                heightMultiplier, heightCurve
            );
            Tree.Init(GameObject.CreatePrimitive(PrimitiveType.Cube));

            MakeBigSquareEditorVer();
            PointsStuff();
        }
    }
    void PointsStuff()
    {
        int category01 = 0, category02 = 0, category03 = 0, category04 = 0, category05 = 0, category06 = 0, category07 = 0, category08 = 0, category09 = 0, category10 = 0;
        int category035 = 0, category045 = 0, category055 = 0, category065 = 0;
        int allCount = allpoints.Count;
        foreach (float value in allpoints)
        {
            if (value < 0.1f)
                category01++;
            else if (value < 0.2f)
                category02++;
            else if (value < 0.3f)
                category03++;
            else if (value < 0.35f)
                category035++;
            else if (value < 0.4f)
                category04++;
            else if (value < 0.45f)
                category045++;
            else if (value < 0.5f)
                category05++;
            else if (value < 0.55f)
                category055++;
            else if (value < 0.6f)
                category06++;
            else if (value < 0.65F)
                category065++;
            else if (value < 0.7f)
                category07++;
            else if (value < 0.8f)
                category08++;
            else if (value < 0.9f)
                category09++;
            else if (value < 1.0f)
                category10++;
        }
        float mean = 0;
        foreach (var v in allpoints)
        {
            mean += (v);
        }
        mean /= allpoints.Count;
        float stdDevi = 0;
        foreach (var v in allpoints)
        {
            stdDevi += (v - mean) * (v - mean);
        }
        stdDevi /= allpoints.Count;
        stdDevi = Mathf.Sqrt(stdDevi);

        Debug.Log("Mean: " + mean + ", stdDevi: " + stdDevi);

        float z10 = -1.282f;
        float z15 = -1.036f;
        // float z20 = -0.842f;
        float z25 = -0.674f;
        // float z30 = -0.524f;
        // float z40 = -0.253f;
        float z50 = 0f;
        // float z60 = +0.253f;
        float z65 = +0.385f;
        // float z70 = +0.524f;
        float z80 = +0.842f;
        // float z90 = +1.282f;
        float z95 = +1.645f;

        float point10 = mean + (z10 * stdDevi);
        float point15 = mean + (z15 * stdDevi);
        float point25 = mean + (z25 * stdDevi);
        float point50 = mean + (z50 * stdDevi);
        float point65 = mean + (z65 * stdDevi);
        float point80 = mean + (z80 * stdDevi);
        float point95 = mean + (z95 * stdDevi);
        Debug.Log("point10: " + point10);
        Debug.Log("point15: " + point15);
        Debug.Log("point25: " + point25);
        Debug.Log("point50: " + point50);
        Debug.Log("point65: " + point65);
        Debug.Log("point80: " + point80);
        Debug.Log("point95: " + point95);
        /*
            10, 20, 25, 50, 65, 80, 95
        */
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
        chunksForEditor = new Chunk[ChunkLoadCountForEditor, ChunkLoadCountForEditor];
        for (int x = 0; x < chunksForEditor.GetLength(0); x++)
        {
            for (int z = 0; z < chunksForEditor.GetLength(1); z++)
            {
                chunksForEditor[x, z] = new Chunk(new Vector3(x, 0, z));
                chunksForEditor[x, z].Generate();
                chunksForEditor[x, z].MakeGameObject();
            }
        }
    }
#endif

}
