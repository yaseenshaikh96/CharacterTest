using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField, Range(2, 20)] private int pointsPerChunk;
    [SerializeField, Range(5, 20)] private float chunkSize; // in world pos
    [SerializeField] private bool isSmoothMesh;
    [SerializeField] private Material mesMaterial;
    [SerializeField] private NoiseData noiseData;


    void Start()
    {
        Chunk.Init
        ( 
            pointsPerChunk, chunkSize, isSmoothMesh, mesMaterial,
            noiseData
        );
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
