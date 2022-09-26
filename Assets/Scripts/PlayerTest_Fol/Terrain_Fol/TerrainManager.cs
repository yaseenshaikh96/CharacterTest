using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainManager : MonoBehaviour
{
    [SerializeField, Range(2, 20)] private int pointsPerChunk;
    [SerializeField, Range(5, 20)] private float chunkSize; // in world pos
    [SerializeField] private Material mesMaterial;

    [SerializeField, Range(1, 100)] private float noiseScale;


    void Start()
    {
        Chunk.Init
        (
            pointsPerChunk, chunkSize, mesMaterial,
            noiseScale
        );
        Chunk chunk = new Chunk(Vector3.zero);
        chunk.Generate();
        chunk.MakeGameObject();
    }


    void Update()
    {

    }
}
