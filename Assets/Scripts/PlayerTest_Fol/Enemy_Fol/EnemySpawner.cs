using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ParentCellNode
{
    public int xIndex, zIndex;
    public Vector3 position = Vector3.zero;
    public bool spawnable = false;
}

public class EnemySpawner : MonoBehaviour
{
    GameObject DebugParentGO;
    [SerializeField] Vector3 testPosition;
    [SerializeField] bool update = false;
    [SerializeField] GameObject EnemyPrefab;
    [SerializeField] public GameObject playerGO;
    [SerializeField] public LayerMask groundLayer;
    
    [SerializeField] public int AIChunkCount; // 3 => 3X3 centered on player
    [SerializeField] public float attackDistance;

    public static int sAIGridSize = 21;


    public Vector2 playerWorldPos2D {get; private set;}
    public Vector3 playerWorldPos {get; private set;}



    Dictionary<Vector3, Chunk> loadedChunks;
    public ParentCellNode[,] parentCellNodes;
    ChunkManager chunkManager;
    public static int sPointsPerChunk;
    public static float sChunkSize;

    void Start()
    {
        DebugParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);

        chunkManager = GameObject.Find("TerrainManagerGO").GetComponent<ChunkManager>();
        sPointsPerChunk = chunkManager.pointsPerChunk;
        sChunkSize = chunkManager.chunkSize;

        loadedChunks = GameObject.Find("TerrainManagerGO").GetComponent<TerrainDynamicLoad>().loadedChunks;

        playerWorldPos = new Vector3();
        playerWorldPos2D = new Vector2();
        parentCellNodes = new ParentCellNode[sPointsPerChunk * AIChunkCount, sPointsPerChunk * AIChunkCount];
        for(int xIndex = 0; xIndex < sPointsPerChunk * AIChunkCount; xIndex++)
        {
            for(int zIndex = 0; zIndex < sPointsPerChunk * AIChunkCount; zIndex ++)
            {
                parentCellNodes[xIndex, zIndex] = new ParentCellNode();
                parentCellNodes[xIndex, zIndex].xIndex = xIndex;
                parentCellNodes[xIndex, zIndex].zIndex = zIndex;
            }
        }
        Enemy_AI.enemySpawner = this;

        // spawn enemies
        //Instantiate(EnemyPrefab, new Vector3(0, 0, -50), Quaternion.identity);
    }

/*
        timeSinceLastLoaded += Time.deltaTime;
        if (timeSinceLastLoaded > timeBtnLoad)
        {
            timeSinceLastLoaded = 0;
*/
    float timeSinceLoad = 0;
    bool hasBeenUpdated = false;
    const float timeBetweenLoad = 4f; 
    void Update()
    {
        timeSinceLoad += Time.deltaTime;
        if (!hasBeenUpdated && timeSinceLoad > timeBetweenLoad)
        {
            hasBeenUpdated = true;
            UpdatePositionNodes();
        }
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
        Destroy(DebugParentGO);
        DebugParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DebugParentGO.name = "EnemySpawnerGO_DebugParent"; 

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

                for(int pointIndexX = 0; pointIndexX < sPointsPerChunk - 1; pointIndexX++)
                {
                    for(int pointIndexZ = 0; pointIndexZ < sPointsPerChunk - 1; pointIndexZ++)
                    {
                        int truePointIndex = (pointIndexX * sPointsPerChunk) + pointIndexZ;
                        parentCellNodes[
                            (xIndex * sPointsPerChunk) + pointIndexX,
                            (zIndex * sPointsPerChunk) + pointIndexZ].position = currentChunk.vertexPositions[truePointIndex];
                        parentCellNodes[
                            (xIndex * sPointsPerChunk) + pointIndexX,
                            (zIndex * sPointsPerChunk) + pointIndexZ].spawnable = currentChunk.spawnablePoints[truePointIndex];
                    }
                }
            }
        }
        for(int xIndex = 0; xIndex < sPointsPerChunk * AIChunkCount; xIndex++)
        {
            for(int zIndex = 0; zIndex < sPointsPerChunk * AIChunkCount; zIndex ++)
            {
                if(parentCellNodes[xIndex, zIndex].spawnable)
                {
                    GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.parent = DebugParentGO.transform;
                    go.transform.position = parentCellNodes[xIndex, zIndex].position;
                    go.GetComponent<Collider>().enabled = false;

                }
            }
        }

    }

    Vector3 GetChunkIndexFromPosition(Vector3 position)
    {
        int x = Mathf.FloorToInt(position.x / sChunkSize);
        int z = Mathf.FloorToInt(position.z / sChunkSize);
        return new Vector3(x,0,z);        
    }

    public ParentCellNode GetParentIndexFromPosition(Vector3 position)
    {
        
        int xIndex, zIndex;
        for(xIndex=0; xIndex<sPointsPerChunk * AIChunkCount; xIndex++ )
        {
            if(Mathf.Abs(position.x - parentCellNodes[xIndex, 0].position.x) < ((sChunkSize / sPointsPerChunk) * 0.8f))
            {
                break;
            }    
        }
        for(zIndex=0; zIndex<sPointsPerChunk * AIChunkCount; zIndex++ )
        {
            if(Mathf.Abs(position.z - parentCellNodes[0, zIndex].position.z) < ((sChunkSize / sPointsPerChunk) * 0.8f))
            {
                break;
            }
        }
        return parentCellNodes[xIndex, zIndex];
        /*
        int[] indexs = new int[2];
        indexs[0] = Mathf.RoundToInt(position.x / (chunkSize / pointsPerChunk));
        indexs[1] = Mathf.RoundToInt(position.z / (chunkSize / pointsPerChunk));
        Debug.Log("Position: " + position + ", x: " + indexs[0] + ", z: " + indexs[1]);
        return indexs;
        */
    }

}