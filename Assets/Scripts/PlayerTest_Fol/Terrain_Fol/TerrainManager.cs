using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField, Range(2, 20)] private int pointsPerChunk;
    [SerializeField, Range(5, 20)] private float chunkSize; // in world pos
    [SerializeField, Range(5, 20)] private float heightMultiplier;
    [SerializeField] private bool isSmoothMesh;
    [SerializeField] private Material mesMaterial;
    [SerializeField] private NoiseData noiseData;
    [SerializeField] private AnimationCurve heightCurve;


    void Start()
    {
        Chunk.Init
        (
            pointsPerChunk, chunkSize, isSmoothMesh, mesMaterial,
            noiseData,
            heightMultiplier, heightCurve
        );

        MakeBigSquare();


    }

    void MakeBigSquare()
    {
        Chunk[,] chunks = new Chunk[5, 5];
        for (int x = 0; x < chunks.GetLength(0); x++)
        {
            for (int z = 0; z < chunks.GetLength(1); z++)
            {
                chunks[x, z] = new Chunk(new Vector3(x, 0, z) * chunkSize);
                chunks[x, z].Generate();
                chunks[x, z].MakeGameObject();
            }
        }
    }

    void MakeLongLineChunk()
    {
        Chunk[] chunks = new Chunk[40];
        for (int i = -20; i < 20; i++)
        {
            chunks[i + 20] = new Chunk(new Vector3(i, 0, 0) * chunkSize);
            chunks[i + 20].Generate();
            chunks[i + 20].MakeGameObject();
        }
    }

    void MakeQuadChunk()
    {
        Chunk chunk = new Chunk(Vector3.zero);
        Chunk chunk2 = new Chunk(new Vector3(-1, 0, 0) * chunkSize);
        Chunk chunk3 = new Chunk(new Vector3(0, 0, -1) * chunkSize);
        Chunk chunk4 = new Chunk(new Vector3(-1, 0, -1) * chunkSize);

        chunk.Generate();
        chunk.MakeGameObject();
        chunk2.Generate();
        chunk2.MakeGameObject();
        chunk3.Generate();
        chunk3.MakeGameObject();
        chunk4.Generate();
        chunk4.MakeGameObject();
    }

    void Update()
    {

    }
}
