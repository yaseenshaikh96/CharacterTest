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
    [SerializeField, Range(5, 500)] private float heightMultiplier;
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

        float z = -2.57f;//-0.82f;// -1.28f;
        float percentile10thPoint = mean + (z * stdDevi);
        Debug.Log("Mean: " + mean + ", stdDevi: " + stdDevi + ", percentile10thPoint: " + percentile10thPoint);


        /*
        Debug.Log(
            $"total Points: {allCount}" +
            $"0.0-0.1: {category01}, {100 * (float)category01 / allCount}% \n" +
            $"0.1-0.2: {category02}, {100 * (float)category02 / allCount}% \n" +
            $"0.2-0.3: {category03}, {100 * (float)category03 / allCount}% \n" +
            $"0.3-0.4: {category04}, {100 * (float)category04 / allCount}% : {category045}, {100 * (float)category045 / allCount}% \n" +
            $"0.4-0.5: {category05}, {100 * (float)category05 / allCount}% : {category055}, {100 * (float)category055 / allCount}% \n" +
            $"0.5-0.6: {category06}, {100 * (float)category06 / allCount}% : {category065}, {100 * (float)category065 / allCount}% \n" +
            $"0.6-0.7: {category07}, {100 * (float)category07 / allCount}% \n" +
            $"0.7-0.8: {category08}, {100 * (float)category08 / allCount}% \n" +
            $"0.8-0.9: {category09}, {100 * (float)category09 / allCount}% \n" +
            $"0.9-1.0: {category10}, {100 * (float)category10 / allCount}% \n"
        );*/
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
