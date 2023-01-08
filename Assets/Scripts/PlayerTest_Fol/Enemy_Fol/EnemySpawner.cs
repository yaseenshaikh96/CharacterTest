using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PositionNode
{
    public Vector3 position = Vector3.zero;
    public bool spawnable = false;
}

public class EnemySpawner : MonoBehaviour
{
    [SerializeField] Vector3 testPosition;
    [SerializeField] bool update = false;
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] public GameObject playerGO;
    [SerializeField] public LayerMask groundLayer;
    
    [SerializeField] public int AIChunkCount; // 3 => 3X3 centered on player
    [SerializeField] public float unloadDistance;
    [SerializeField] public int AIGridSize = 20;
    [SerializeField] public int chasingDistance;
    [SerializeField] public float attackDistance;

    public Vector2 playerWorldPos2D {get; private set;}
    public Vector3 playerWorldPos {get; private set;}


    Dictionary<Vector3, Chunk> loadedChunks;
    public PositionNode[,] positionNodes;
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
        positionNodes = new PositionNode[pointsPerChunk * AIChunkCount, pointsPerChunk * AIChunkCount];
        for(int xIndex = 0; xIndex < pointsPerChunk * AIChunkCount; xIndex++)
        {
            for(int zIndex = 0; zIndex < pointsPerChunk * AIChunkCount; zIndex ++)
            {
                positionNodes[xIndex, zIndex] = new PositionNode();
            }
        }
        Enemy_AI.enemySpawner = this;

        // spawn enemies
        //Instantiate(EnemyPrefab, new Vector3(0, 0, -50), Quaternion.identity);
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
            GetIndexFromPosition(testPosition);
        }
    }

    void UpdatePositionNodes()
    {
        loadedChunks = GameObject.Find("TerrainManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        Vector3 playerChunkIndex = GetChunkIndexFromPosition(playerWorldPos);
        
        for(int xIndex = 0; xIndex < AIChunkCount; xIndex++)
        {
            for(int zIndex = 0; zIndex < AIChunkCount; zIndex ++)
            {
                int chunkIndexX = (int)playerChunkIndex.x - (int)((AIChunkCount-1)/2) + xIndex;
                int chunkIndexZ = (int)playerChunkIndex.z - (int)((AIChunkCount-1)/2) + zIndex;
                
                Vector3 currentChunkPos = new Vector3(chunkIndexX, 0, chunkIndexZ);
                Chunk currentChunk;
                bool success = loadedChunks.TryGetValue(currentChunkPos, out currentChunk);

                for(int pointIndexX = 0; pointIndexX < pointsPerChunk - 1; pointIndexX++)
                {
                    for(int pointIndexZ = 0; pointIndexZ < pointsPerChunk - 1; pointIndexZ++)
                    {
                        int truePointIndex = (pointIndexX * pointsPerChunk) + pointIndexZ;
                        positionNodes[
                            (xIndex * pointsPerChunk) + pointIndexX,
                            (zIndex * pointsPerChunk) + pointIndexZ].position = currentChunk.vertexPositions[truePointIndex];
                        positionNodes[
                            (xIndex * pointsPerChunk) + pointIndexX,
                            (zIndex * pointsPerChunk) + pointIndexZ].spawnable = currentChunk.spawnablePoints[truePointIndex];
                    }
                }
            }
        }
        for(int xIndex = 0; xIndex < pointsPerChunk * AIChunkCount; xIndex++)
        {
            for(int zIndex = 0; zIndex < pointsPerChunk * AIChunkCount; zIndex ++)
            {
                if(positionNodes[xIndex, zIndex].spawnable)
                {
                    GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.position = positionNodes[xIndex, zIndex].position;
                    go.GetComponent<Collider>().enabled = false;

                }
            }
        }

    }

    Vector3 GetChunkIndexFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / chunkSize);
        int z = Mathf.FloorToInt(position.z / chunkSize);
        return new Vector3(x,0,z);        
    }

    public int[] GetIndexFromPosition(Vector3 position)
    {

        float xGridPos = (position.x / (chunkSize / pointsPerChunk));
        float zGridPos =  (position.z / (chunkSize / pointsPerChunk));
        int[] indices = new int[2];

        for(int xIndex=0; xIndex<pointsPerChunk * AIChunkCount; xIndex++ )
        {
            if(Mathf.Abs(position.x - positionNodes[xIndex, 0].position.x) < ((chunkSize / pointsPerChunk) / 1.5f))
                indices[0] = xIndex;
        }
        for(int zIndex=0; zIndex<pointsPerChunk * AIChunkCount; zIndex++ )
        {
            if(Mathf.Abs(position.z - positionNodes[0, zIndex].position.z) < ((chunkSize / pointsPerChunk) / 1.5f))
                indices[1] = zIndex;
        }
        return indices;
        /*
        int[] indexs = new int[2];
        indexs[0] = Mathf.RoundToInt(position.x / (chunkSize / pointsPerChunk));
        indexs[1] = Mathf.RoundToInt(position.z / (chunkSize / pointsPerChunk));
        Debug.Log("Position: " + position + ", x: " + indexs[0] + ", z: " + indexs[1]);
        return indexs;
        */
    }

}