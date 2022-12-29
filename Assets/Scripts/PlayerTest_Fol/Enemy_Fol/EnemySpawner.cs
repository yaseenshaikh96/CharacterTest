using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] public GameObject playerGO;
    [SerializeField] public LayerMask groundLayer;
    
    [SerializeField] public int AIChunkCount;
    [SerializeField] public float unloadDistance;
    [SerializeField] public int chasingDistance;
    [SerializeField] public float attackDistance;

    public Vector2 playerWorldPos2D {get; private set;}
    public Vector3 playerWorldPos {get; private set;}

    Dictionary<Vector3, Chunk> loadedChunks;
    public Vector3[,] positionNodes;
    ChunkManager chunkManager;
    int pointsPerChunk;
    float chunkSize;

    void Start()
    {

        chunkManager = GameObject.Find("ChunkManagerGO").GetComponent<ChunkManager>();
        pointsPerChunk = chunkManager.pointsPerChunk;
        chunkSize = chunkManager.chunkSize;

        loadedChunks = GameObject.Find("ChunkManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        // spawn enemies
        playerWorldPos = new Vector3();
        playerWorldPos2D = new Vector2();
        positionNodes = new Vector3[pointsPerChunk * AIChunkCount, pointsPerChunk * AIChunkCount];
        
        Enemy_AI.enemySpawner = this;


        Instantiate(EnemyPrefab, new Vector3(0, 0, -50), Quaternion.identity);
    }

    void Update()
    {
        // update player pos
        playerWorldPos = playerGO.transform.position;
        playerWorldPos2D = new Vector2(playerGO.transform.position.x, playerGO.transform.position.z);
    }

    void UpdatePositionNodes()
    {
        loadedChunks = GameObject.Find("ChunkManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        Vector3 playerChunkIndex = GetChunkIndexFromPosition(playerWorldPos);

        int chunkXMinIndex = (int)playerChunkIndex.x - (AIChunkCount * (int)chunkSize);
        int chunkXMaxIndex = (int)playerChunkIndex.x + (AIChunkCount * (int)chunkSize);

        int chunkZMinIndex = (int)playerChunkIndex.z - (AIChunkCount * (int)chunkSize);
        int chunkZMaxIndex = (int)playerChunkIndex.z + (AIChunkCount * (int)chunkSize);

        for(int xIndex = chunkXMinIndex; xIndex <= chunkXMaxIndex; xIndex += (int)chunkSize)
        {
            for(int zIndex = chunkZMinIndex; zIndex <= chunkZMaxIndex; zIndex += (int)chunkSize)
            {
                Vector3 currentChunkPos = new Vector3(xIndex, 0,zIndex);
                Chunk currentChunk;
                bool success = loadedChunks.TryGetValue(currentChunkPos, out currentChunk);
                if(success)
                {
                    currentChunk.vertexPositions;
                    
                }
                    
            }
        }


    }

    Vector3 GetChunkIndexFromPosition(Vector3 position)
    {
        int x = (int)(position.x % chunkSize);
        int z = (int)(position.z % chunkSize);
        return new Vector3(x, 0, z);
    }
}
