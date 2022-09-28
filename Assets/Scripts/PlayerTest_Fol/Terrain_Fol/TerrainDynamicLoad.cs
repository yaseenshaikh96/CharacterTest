using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainDynamicLoad : MonoBehaviour
{
    [SerializeField] private GameObject playerGO;
    [SerializeField] private int loadRadius;

    private Dictionary<Vector3, Chunk> loadedChunks;

    void Start()
    {
        loadedChunks = new Dictionary<Vector3, Chunk>();
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
            LoadChunks();
        }
    }

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
            if(!newLoadedChunks.ContainsValue(chunk.Value))
                chunk.Value.Delete();
        }

        loadedChunks = newLoadedChunks;

    }
}
