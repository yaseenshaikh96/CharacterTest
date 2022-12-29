using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] bool update = false;
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] public GameObject playerGO;
    [SerializeField] public LayerMask groundLayer;
    
    [SerializeField] public int AIChunkCount; // 3 => 3X3 centered on player
    [SerializeField] public float unloadDistance;
    [SerializeField] public int chasingDistance;
    [SerializeField] public float attackDistance;

    public Vector2 playerWorldPos2D {get; private set;}
    public Vector3 playerWorldPos {get; private set;}


    Dictionary<Vector3, bool> positionNodesWithSpawns;
    Dictionary<Vector3, Chunk> loadedChunks;
    public Vector3[,] positionNodes;
    ChunkManager chunkManager;
    int pointsPerChunk;
    float chunkSize;

    void Start()
    {

        chunkManager = GameObject.Find("TerrainManagerGO").GetComponent<ChunkManager>();
        pointsPerChunk = chunkManager.pointsPerChunk;
        chunkSize = chunkManager.chunkSize;

        loadedChunks = GameObject.Find("TerrainManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        playerWorldPos = new Vector3();
        playerWorldPos2D = new Vector2();
        positionNodes = new Vector3[pointsPerChunk * AIChunkCount, pointsPerChunk * AIChunkCount];
        positionNodesWithSpawns = new Dictionary<Vector3, bool>((2 * pointsPerChunk) * (2 * AIChunkCount));

        Enemy_AI.enemySpawner = this;


        // spawn enemies
        Instantiate(EnemyPrefab, new Vector3(0, 0, -50), Quaternion.identity);
    }

    void Update()
    {

        // update player pos
        playerWorldPos = playerGO.transform.position;
        playerWorldPos2D = new Vector2(playerGO.transform.position.x, playerGO.transform.position.z);
        
        if(update)
        {
            update = false;
            UpdatePositionNodes();
        }
    }

    void UpdatePositionNodes()
    {
        loadedChunks = GameObject.Find("TerrainManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        Vector3 playerChunkIndex = GetChunkIndexFromPosition(playerWorldPos);

        float chunkXMinIndex = playerChunkIndex.x - ((AIChunkCount / 2) * chunkSize);
        float chunkXMaxIndex = playerChunkIndex.x + ((AIChunkCount / 2) * chunkSize);

        float chunkZMinIndex = playerChunkIndex.z - ((AIChunkCount / 2) * chunkSize);
        float chunkZMaxIndex = playerChunkIndex.z + ((AIChunkCount / 2) * chunkSize);

        Debug.Log("player chunk index: " + playerChunkIndex);

        for(float xIndex = chunkXMinIndex; xIndex <= chunkXMaxIndex; xIndex += chunkSize)
        {
            for(float zIndex = chunkZMinIndex; zIndex <= chunkZMaxIndex; zIndex += chunkSize)
            {
                Vector3 currentChunkPos = new Vector3(xIndex, 0, zIndex) / chunkSize;
                Chunk currentChunk;
                bool success = loadedChunks.TryGetValue(currentChunkPos, out currentChunk);
                
                Debug.Log("position: " + currentChunkPos + " success: " + success);
                
            
                for(int pointIndexX = 0; pointIndexX < pointsPerChunk - 1; pointIndexX++)
                {
                    for(int pointIndexZ = 0; pointIndexZ < pointsPerChunk - 1; pointIndexZ++)
                    {
                        int truePointIndex = (pointIndexX * pointsPerChunk) + pointIndexZ;
                        positionNodesWithSpawns.Add(currentChunk.vertexPositions[truePointIndex], currentChunk.spawnablePoints[truePointIndex]);

                    }
                }
        
            }
        }

        foreach(var node in positionNodesWithSpawns)
        {
            if(node.Value)
            {
                GameObject go = GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.transform.position = node.Key;
            }
        }

    }

    Vector3 GetChunkIndexFromPosition(Vector3 position)
    {
        return new Vector3(0,0,0);
        // if x = 243, z = -234, chunkSize = 100
        // out => (200, 0, -200)
        
        //int x = (int)(position.x / chunkSize);
        //int z = (int)(position.z / chunkSize);

        //return new Vector3(x, 0, z) * chunkSize;
    }
}