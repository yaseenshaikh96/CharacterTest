using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType
{
    flat, smooth
}

public class Chunk
{
    public static GameObject sPlayerGO { get; private set; }
    public static GameObject sParentGO { get; private set; }
    public static LayerMask sGroundLayer { get; private set; }
    public static float sChunkSize { get; private set; }
    public static int sPointsPerChunk { get; private set; }
    public static Material sMeshMaterial { get; private set; }
    public static MeshType sMeshType { get; private set; }
    public static float sHeightMultiplier { get; private set; }
    public static AnimationCurve sHeightCurve { get; private set; }
    static NoiseData mNoiseData;
    static float maxNoiseHeight;
    static int sVerticesPosIndexCount, sTriangleIndexCount;
    //---------------------------------------------------------------------------------//
    public bool heightDataLoaded { get; private set; }
    public bool gameObjectMade { get; private set; }
    public Vector3 mWorldPos { get; private set; } // unique ID
    public Vector3 mChunkPos { get; private set; }
    public float timeWhenCreated;
    //---------------------------------------------------------------------------------//
    Vector3[] vertexPositions;
    Vector3[] vertices;
    int[] triangles;
    Vector3[] normals;
    public GameObject meshGO;
    MeshFilter mMeshFilter;
    MeshRenderer mMeshRenderer;
    MeshCollider mMeshCollider;
    float[] heightData; // height of each vertex

    //---------------------------------------------------------------------------------//
    public static void Init
    (
        GameObject playerGO, GameObject parentGO, LayerMask groundLayer,
        int pointsPerChunk, float chunkSize, MeshType meshType, Material meshMaterial,
        NoiseData noiseData,
        float heightMultiplier, AnimationCurve heightCurve
    )
    {
        sPlayerGO = playerGO;
        sParentGO = parentGO;
        sGroundLayer = groundLayer;

        sPointsPerChunk = pointsPerChunk;
        sChunkSize = chunkSize;
        sMeshType = meshType;
        sMeshMaterial = meshMaterial;

        mNoiseData = noiseData;

        sHeightMultiplier = heightMultiplier;
        sHeightCurve = heightCurve;

        maxNoiseHeight = CalculateMaxNoiseHeight();
        sVerticesPosIndexCount = sPointsPerChunk * sPointsPerChunk;
        sTriangleIndexCount = 6 * (sPointsPerChunk - 1) * (sPointsPerChunk - 1);
    }
    public Chunk(Vector3 chunkPos)
    {
        timeWhenCreated = Time.time;
        mWorldPos = chunkPos * sChunkSize;
        mChunkPos = chunkPos;

        heightDataLoaded = false;
        gameObjectMade = false;

    }
    public void Delete()
    {
        UnityEngine.GameObject.Destroy(meshGO);
    }
    ~Chunk()
    {
        UnityEngine.GameObject.Destroy(meshGO);
    }
    //---------------------------------------------------------------------------------------------------------------------------//
    public void Generate()
    {
        heightData = new float[sVerticesPosIndexCount];
        vertexPositions = new Vector3[sVerticesPosIndexCount];
        triangles = new int[sTriangleIndexCount];

        if (sMeshType == MeshType.flat)
            vertices = new Vector3[sTriangleIndexCount];
        else if (sMeshType == MeshType.smooth)
            vertices = new Vector3[sVerticesPosIndexCount];

        MakeHeightData();

        for (int xIndex = 0; xIndex < sPointsPerChunk; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk; zIndex++)
            {
                int currentIndex = (xIndex * sPointsPerChunk) + zIndex;

                float xPos = ((float)(xIndex) / (sPointsPerChunk - 1)) * sChunkSize;
                float zPos = ((float)(zIndex) / (sPointsPerChunk - 1)) * sChunkSize;
                float yPos = heightData[currentIndex];

                vertexPositions[currentIndex] = (new Vector3(xPos, yPos, zPos)) + mWorldPos;
            }
        }

        if (sMeshType == MeshType.flat)
            FlatMesh();
        else if (sMeshType == MeshType.smooth)
            SmoothMesh();

        heightDataLoaded = true;
    }

    public void MakeGameObject()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.triangles = triangles;

        mesh.RecalculateNormals();

        meshGO = new GameObject($"Chunk {mWorldPos.x} {mWorldPos.z}");

        mMeshFilter = meshGO.AddComponent<MeshFilter>();
        mMeshFilter.mesh = mesh;

        mMeshRenderer = meshGO.AddComponent<MeshRenderer>();
        mMeshRenderer.material = sMeshMaterial;

        meshGO.transform.SetParent(sParentGO.transform);

        mMeshCollider = meshGO.AddComponent<MeshCollider>();
        mMeshCollider.sharedMesh = mMeshFilter.sharedMesh;

        meshGO.layer = 8; //TODO:

        gameObjectMade = true;
    }
    //-------------------------------------------------------------------------------//
    void FlatMesh()
    {
        int currentTriangleCount = 0;
        for (int xIndex = 0; xIndex < sPointsPerChunk - 1; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk - 1; zIndex++)
            {
                int currentVertexIndex = (xIndex * sPointsPerChunk) + zIndex;

                int currentPointIndex = currentVertexIndex;
                int rightPointIndex = currentPointIndex + sPointsPerChunk;
                int topPointIndex = currentPointIndex + 1;
                int topRightPointIndex = currentPointIndex + sPointsPerChunk + 1;

                vertices[currentTriangleCount + 0] = vertexPositions[currentPointIndex];
                vertices[currentTriangleCount + 1] = vertexPositions[topPointIndex];
                vertices[currentTriangleCount + 2] = vertexPositions[topRightPointIndex];

                vertices[currentTriangleCount + 3] = vertexPositions[currentPointIndex];
                vertices[currentTriangleCount + 4] = vertexPositions[topRightPointIndex];
                vertices[currentTriangleCount + 5] = vertexPositions[rightPointIndex];

                // triangle1
                triangles[currentTriangleCount + 0] = currentTriangleCount + 0;
                triangles[currentTriangleCount + 1] = currentTriangleCount + 1;
                triangles[currentTriangleCount + 2] = currentTriangleCount + 2;

                // triangle2
                triangles[currentTriangleCount + 3] = currentTriangleCount + 3;
                triangles[currentTriangleCount + 4] = currentTriangleCount + 4;
                triangles[currentTriangleCount + 5] = currentTriangleCount + 5;

                currentTriangleCount += 6;
            }
        }
    }
    void SmoothMesh()
    {
        for (int index = 0; index < sVerticesPosIndexCount; index++)
            vertices[index] = vertexPositions[index];

        int currentTriangleIndex = 0;
        for (int xIndex = 0; xIndex < sPointsPerChunk - 1; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk - 1; zIndex++)
            {
                int currentVertexIndex = (xIndex * sPointsPerChunk) + zIndex;

                int currentPointIndex = currentVertexIndex;
                int rightPointIndex = currentPointIndex + sPointsPerChunk;
                int topPointIndex = currentPointIndex + 1;
                int topRightPointIndex = currentPointIndex + sPointsPerChunk + 1;

                //triangle 1
                triangles[currentTriangleIndex + 0] = currentPointIndex;
                triangles[currentTriangleIndex + 1] = topPointIndex;
                triangles[currentTriangleIndex + 2] = topRightPointIndex;

                //triangle 2
                triangles[currentTriangleIndex + 3] = currentPointIndex;
                triangles[currentTriangleIndex + 4] = topRightPointIndex;
                triangles[currentTriangleIndex + 5] = rightPointIndex;

                currentTriangleIndex += 6;
            }
        }
    }

    private void MakeHeightData()
    {
        System.Random prng = new System.Random(mNoiseData.seed);

        Vector2[] octaveOffsets = new Vector2[mNoiseData.octave];

        for (int i = 0; i < mNoiseData.octave; i++)
        {
            octaveOffsets[i].x = prng.Next(-10000, 10000) + mNoiseData.offset.x;
            octaveOffsets[i].y = prng.Next(-10000, 10000) + mNoiseData.offset.y;
        }

        for (int xIndex = 0; xIndex < sPointsPerChunk; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk; zIndex++)
            {
                int currentIndex = (xIndex * sPointsPerChunk) + zIndex;

                float xPos = ((float)xIndex / (sPointsPerChunk - 1)) * sChunkSize;
                float zPos = ((float)zIndex / (sPointsPerChunk - 1)) * sChunkSize;
                xPos += mWorldPos.x; // worldPos
                zPos += mWorldPos.z; // worldPos

                float frequency = 1;
                float amplitude = 1;

                float noiseForAllOct = 0;
                for (int octIndex = 0; octIndex < mNoiseData.octave; octIndex++)
                {
                    float xPosAdj = (xPos + octaveOffsets[octIndex].x) / mNoiseData.scale * frequency;
                    float zPosAdj = (zPos + octaveOffsets[octIndex].y) / mNoiseData.scale * frequency;

                    float noiseForThisOct = Mathf.PerlinNoise(xPosAdj, zPosAdj);
                    noiseForAllOct += noiseForThisOct * amplitude;

                    frequency *= mNoiseData.lacunarity;
                    amplitude *= mNoiseData.presistance;
                }

                float normalizedNoise = Remap(noiseForAllOct, 0, maxNoiseHeight, 0, 1);
                float normalizedNoiseCurveAdj = normalizedNoise * sHeightCurve.Evaluate(normalizedNoise);

                heightData[currentIndex] = normalizedNoiseCurveAdj * sHeightMultiplier;
            }
        }
    }

    //----------------------------------------------------------------------------------------//
    static float CalculateMaxNoiseHeight()
    {
        float maxHeight = 0;
        float amplitude = 1;
        for (int octIndex = 0; octIndex < mNoiseData.octave; octIndex++)
        {
            float thisOctHeight = 1 * amplitude;
            maxHeight += thisOctHeight;
            amplitude *= mNoiseData.presistance;
        }
        return maxHeight;
    }
    static float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
        return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
    }
}
