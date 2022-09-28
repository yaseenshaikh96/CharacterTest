using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerrainDynamicLoad : MonoBehaviour
{
    [SerializeField] private GameObject playerGO;
    [SerializeField] private int loadRadius;

    private Dictionary<Vector3, Chunk> allLoadedChunks;
    private Dictionary<Vector3, Chunk> allCreatedChunks;

    void Start()
    {
        allLoadedChunks = new Dictionary<Vector3, Chunk>(loadRadius * loadRadius);
        allCreatedChunks = new Dictionary<Vector3, Chunk>(loadRadius * loadRadius);
    }

    float timeSinceLastLoaded = 0;
    const float timeBtnLoad = 0.5f; // sec
    void Update()
    {
        timeSinceLastLoaded += Time.deltaTime;
        if (timeSinceLastLoaded > timeBtnLoad)
        {
            Debug.Log("Ran");
            timeSinceLastLoaded = 0;

            LoadChunksMultiThreaded();
        }
    }
    void LoadChunksMultiThreaded()
    {
        int xPosPlayerInChunk = Mathf.RoundToInt(playerGO.transform.position.x / Chunk.sChunkSize);
        int zPosPlayerInChunk = Mathf.RoundToInt(playerGO.transform.position.z / Chunk.sChunkSize);

        for (int xIndex = -loadRadius; xIndex < loadRadius; xIndex++)
        {
            for (int zIndex = -loadRadius; zIndex < loadRadius; zIndex++)
            {
                float xPos = xIndex + xPosPlayerInChunk;
                float zPos = zIndex + zPosPlayerInChunk;
                Vector3 currentChunkPos = new Vector3(xPos, 0, zPos);

                Chunk currentChunk;
                bool chunkLoaded = allLoadedChunks.TryGetValue(currentChunkPos, out currentChunk);
                bool chunkCreated;
                if (!chunkLoaded)
                {
                    if (currentChunk != null && currentChunk.isLoaded)
                    {
                        allLoadedChunks.Add(currentChunkPos, currentChunk);
                    }

                    chunkCreated = allCreatedChunks.TryGetValue(currentChunkPos, out currentChunk);
                    if (!chunkCreated)
                    {
                        currentChunk = new Chunk(currentChunkPos);
                        allCreatedChunks.Add(currentChunkPos, currentChunk);
                        ThreadStart threadStart = new ThreadStart(currentChunk.Make);
                        Thread thread = new Thread(threadStart);
                    }
                }
            }
        }

        foreach (Transform chunkT in Chunk.sParentGO.transform)
        {
            if (!IsChunkInRange(chunkT))
            {
                allLoadedChunks.Remove(chunkT.position / Chunk.sChunkSize);
                allCreatedChunks.Remove(chunkT.position / Chunk.sChunkSize);
                Destroy(chunkT);
            }
        }

        bool IsChunkInRange(Transform chunkT)
        {
            float chunkX = chunkT.position.x;
            float chunkZ = chunkT.position.z;

            float playerX = playerGO.transform.position.x;
            float playerZ = playerGO.transform.position.z;

            if (
                chunkX > playerX + (loadRadius * Chunk.sChunkSize) ||
                chunkX < playerX - (loadRadius * Chunk.sChunkSize) ||
                chunkZ > playerZ + (loadRadius * Chunk.sChunkSize) ||
                chunkZ < playerZ - (loadRadius * Chunk.sChunkSize)
            )
                return false;
            return true;

        }

    }
    /*
        private Dictionary<Vector3, Chunk> loadedChunks;
        void LoadChunks()
        {

            int xPosPlayerInChunk = Mathf.RoundToInt(playerGO.transform.position.x / Chunk.sChunkSize);
            int zPosPlayerInChunk = Mathf.RoundToInt(playerGO.transform.position.z / Chunk.sChunkSize);

            Dictionary<Vector3, Chunk> newLoadedChunks = new Dictionary<Vector3, Chunk>();
            for (int xIndex = -loadRadius; xIndex < loadRadius; xIndex++)
            {
                for (int zIndex = -loadRadius; zIndex < loadRadius; zIndex++)
                {
                    float xPos = xIndex + xPosPlayerInChunk;
                    float zPos = zIndex + zPosPlayerInChunk;

                    Vector3 currentChunkPos = new Vector3(xPos, 0, zPos);
                    Chunk newChunk;
                    if (!loadedChunks.ContainsKey(currentChunkPos))
                    {
                        newChunk = new Chunk(currentChunkPos);
                        newChunk.Generate();
                        newChunk.MakeGameObject();
                        newChunk.AddCollider();
                        loadedChunks.Add(currentChunkPos, newChunk);
                    }
                    else
                    {
                        loadedChunks.TryGetValue(currentChunkPos, out newChunk);
                    }
                    newLoadedChunks.Add(currentChunkPos, newChunk);

                }
            }

            foreach (var chunk in loadedChunks)
            {
                if (!newLoadedChunks.ContainsValue(chunk.Value))
                    chunk.Value.Delete();
            }

            loadedChunks = newLoadedChunks;

        }
    */
}
