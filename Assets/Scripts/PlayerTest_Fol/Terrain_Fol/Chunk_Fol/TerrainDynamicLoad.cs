using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;

public class TerrainDynamicLoad : MonoBehaviour
{
    [SerializeField] private GameObject playerGO;
    [SerializeField] private int loadRadius;
    [SerializeField] private float colliderRadius;

    System.Action ptrToLoadColliderFunc;

    //---------------------------------------------------------------------
    int xPosPlayerInChunk, zPosPlayerInChunk;
    Vector3 playerWorldPos;
    Dictionary<Vector3, Chunk> loadedChunks, newLoadedChunks;
    void Start()
    {
        loadedChunks = new Dictionary<Vector3, Chunk>();
        newLoadedChunks = new Dictionary<Vector3, Chunk>();

            playerWorldPos = playerGO.transform.position;
            xPosPlayerInChunk = Mathf.RoundToInt(playerWorldPos.x / Chunk.sChunkSize);
            zPosPlayerInChunk = Mathf.RoundToInt(playerWorldPos.z / Chunk.sChunkSize);

            LoadChunksMultiThreaded();
    }

    float timeSinceLastLoaded = 0;
    const float timeBtnLoad = 1f; // sec
    void Update()
    {
        timeSinceLastLoaded += Time.deltaTime;
        if (timeSinceLastLoaded > timeBtnLoad)
        {
            timeSinceLastLoaded = 0;

            playerWorldPos = playerGO.transform.position;
            xPosPlayerInChunk = Mathf.RoundToInt(playerWorldPos.x / Chunk.sChunkSize);
            zPosPlayerInChunk = Mathf.RoundToInt(playerWorldPos.z / Chunk.sChunkSize);

            LoadChunksMultiThreaded();
            LoadColliders();
        }
    }
    void LoadChunksMultiThreaded()
    {
        foreach (var chunk in loadedChunks)
        {
            if (!chunk.Value.gameObjectMade)
            {
                if (chunk.Value.heightDataLoaded)
                {
                    chunk.Value.MakeGameObject();
                }
                else
                {
                    // Debug.Log("T: " + (Time.time - chunk.Value.timeWhenCreated));
                    if (Time.time - chunk.Value.timeWhenCreated > 1)
                    {
                        // Debug.Log("Redoing");
                        chunk.Value.timeWhenCreated = Time.time;
                        ThreadStart threadStart = new ThreadStart(chunk.Value.Generate);
                        Thread thread = new Thread(threadStart);
                        thread.Start();
                    }
                }
            }
        }

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

                    ThreadStart threadStart = new ThreadStart(newChunk.Generate);
                    Thread thread = new Thread(threadStart);
                    thread.Start();

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
    void LoadColliders()
    {
        foreach (var chunk in loadedChunks)
        {
            
            Vector2 chunkWorldpos2D = new Vector2(chunk.Value.mWorldPosCentered.x,chunk.Value.mWorldPosCentered.z);
            Vector3 playerWorldPos2D = new Vector2(playerWorldPos.x, playerWorldPos.z);
            if (Vector2.Distance(chunkWorldpos2D, playerWorldPos2D) > colliderRadius)
            {
                chunk.Value.RemoveCollider();
            }
            else
                chunk.Value.AddCollider();
        }

    }
}
/*
        Vector3 playerPosInChunk = new Vector3(xPosPlayerInChunk, 0, zPosPlayerInChunk);
        foreach (var chunk in loadedChunks)
        {
            chunk.Value.RemoveCollider();
        }
        for (int x = -colliderRadius; x < colliderRadius; x++)
        {
            for (int z = -colliderRadius; z < colliderRadius; z++)
            {
                Vector3 currentChunkPos = playerPosInChunk + new Vector3(x, 0, z);
                Chunk currentChunk;
                loadedChunks.TryGetValue(currentChunkPos, out currentChunk);
                if(currentChunk != null)
                {   
                    currentChunk.AddCollider();
                }
            }
        }

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
