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
    public static Material sWaterMaterial { get; private set; }
    public static MeshType sMeshType { get; private set; }
    public static float sHeightMultiplier { get; private set; }
    public static AnimationCurve sHeightCurve { get; private set; }
    public static Color[] sColorBank;
    static NoiseData mNoiseData;
    static float sMaxNoiseHeight;
    static int sVerticesPosIndexCount, sTriangleIndexCount;
    //---------------------------------------------------------------------------------//
    public bool heightDataLoaded { get; private set; }
    public bool gameObjectMade { get; private set; }
    public Vector3 mWorldPos { get; private set; } // unique ID
    public Vector3 mChunkPos { get; private set; }
    public float timeWhenCreated;
    //---------------------------------------------------------------------------------//
    List<GameObject> spawnableGOs;
    GameObject waterParent;
    Vector3[] vertexPositions;
    bool[] spawnablePoints;
    Vector3[] vertices;
    int[] triangles;
    Color[] colors;
    Vector3[] normals;
    public GameObject meshGO;
    MeshFilter mMeshFilter;
    MeshRenderer mMeshRenderer;
    MeshCollider mMeshCollider;
    float[] heightDataNormalized; // height of each vertex

    //---------------------------------------------------------------------------------//
    public static void Init
    (
        GameObject playerGO, GameObject parentGO, LayerMask groundLayer,
        int pointsPerChunk, float chunkSize, MeshType meshType, Material meshMaterial, Material waterMaterial,
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
        sWaterMaterial = waterMaterial;

        mNoiseData = noiseData;

        sHeightMultiplier = heightMultiplier;
        sHeightCurve = heightCurve;

        sMaxNoiseHeight = CalculateMaxNoiseHeight();
        MakeStaticColors();

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
        foreach (var spawn in spawnableGOs)
            UnityEngine.GameObject.Destroy(spawn);
        UnityEngine.GameObject.Destroy(waterParent);
        UnityEngine.GameObject.Destroy(meshGO);
    }
    ~Chunk()
    {
        Delete();
    }
    //---------------------------------------------------------------------------------------------------------------------------//
    public void Generate()
    {
        heightDataNormalized = new float[sVerticesPosIndexCount];
        vertexPositions = new Vector3[sVerticesPosIndexCount];
        triangles = new int[sTriangleIndexCount];
        colors = new Color[sTriangleIndexCount];
        spawnablePoints = new bool[sVerticesPosIndexCount];
        spawnableGOs = new List<GameObject>();

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
                float yPos = heightDataNormalized[currentIndex] * sHeightMultiplier; // heightCure.Evaluate();

                vertexPositions[currentIndex] = (new Vector3(xPos, yPos, zPos)) + mWorldPos;
            }
        }

        if (sMeshType == MeshType.flat)
        {
            FlatMesh();
        }
        else if (sMeshType == MeshType.smooth)
        {
            SmoothMesh();
        }

        FindSpawnablePoints();
        heightDataLoaded = true;
    }

    public void MakeGameObject()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.triangles = triangles;
        if (colors != null)
            mesh.SetColors(colors);
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

        MakeWaterMesh();
        SpawnSpawnable();
        gameObjectMade = true;

    }
    //-------------------------------------------------------------------------------//
    void SpawnSpawnable()
    {
        Random.InitState(mNoiseData.seed);
        for (int i = 0; i < spawnablePoints.Length; i++)
        {
            if (spawnablePoints[i] && Random.value < 0.2f)
            {
                Tree tree = new Tree(vertexPositions[i], meshGO);
            }
        }
    }
    void FindSpawnablePoints()
    {
        float[] neighbourPointsY = new float[8];
        for (int xIndex = 1; xIndex < sPointsPerChunk - 1; xIndex++)
        {
            for (int zIndex = 1; zIndex < sPointsPerChunk - 1; zIndex++)
            {
                int currentIndex = (xIndex * sPointsPerChunk) + zIndex;
                Vector3 currentPoint = vertexPositions[currentIndex];
                neighbourPointsY[0] = vertexPositions[currentIndex + 1].y;
                neighbourPointsY[1] = vertexPositions[currentIndex - 1].y;

                neighbourPointsY[2] = vertexPositions[currentIndex + sPointsPerChunk].y;
                neighbourPointsY[3] = vertexPositions[currentIndex - sPointsPerChunk].y;

                neighbourPointsY[4] = vertexPositions[currentIndex - sPointsPerChunk + 1].y;
                neighbourPointsY[5] = vertexPositions[currentIndex - sPointsPerChunk - 1].y;

                neighbourPointsY[6] = vertexPositions[currentIndex + sPointsPerChunk + 1].y;
                neighbourPointsY[7] = vertexPositions[currentIndex + sPointsPerChunk - 1].y;

                float deviation = FindDeviation(neighbourPointsY);
                if (
                    deviation < 0.5f &&
                    currentPoint.y > 0.22f * sHeightMultiplier &&
                    currentPoint.y < 0.28f * sHeightMultiplier
                )
                    spawnablePoints[currentIndex] = true;
                else
                    spawnablePoints[currentIndex] = false;
            }
        }

        float FindDeviation(params float[] values)
        {
            float avg = 0;
            for (int i = 0; i < values.Length; i++)
                avg += values[i];

            avg /= values.Length;
            float deviation = 0;
            for (int i = 0; i < values.Length; i++)
                deviation = (values[i] - avg) * (values[i] - avg);

            return Mathf.Sqrt(deviation / values.Length);
        }
    }
    void MakeWaterMesh()
    {
        waterParent = new GameObject("waterParent");
        waterParent.transform.parent = meshGO.transform;

        waterParent.transform.position = mWorldPos;
        GameObject waterGO = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad);

        waterGO.transform.Rotate(Vector3.right * 90);
        waterGO.transform.localScale = Vector3.one * sChunkSize;
        waterGO.transform.parent = waterParent.transform;
        waterGO.transform.localPosition = new Vector3(sChunkSize / 2, sHeightMultiplier * 0.2f, sChunkSize / 2);
        waterGO.GetComponent<Renderer>().material = sWaterMaterial;
    }
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

                // Color color1 = VertexColorFromTriangle(
                //     vertexPositions[currentPointIndex],
                //     vertexPositions[topPointIndex],
                //     vertexPositions[topRightPointIndex]
                // );
                // Color color2 = VertexColorFromTriangle(
                //     vertexPositions[currentPointIndex],
                //     vertexPositions[topRightPointIndex],
                //     vertexPositions[rightPointIndex]
                // );

                Color color1 = ColorFromHeight(
                    heightDataNormalized[currentPointIndex],
                    heightDataNormalized[topPointIndex],
                    heightDataNormalized[topRightPointIndex]
                );
                Color color2 = ColorFromHeight(
                    heightDataNormalized[currentPointIndex],
                    heightDataNormalized[topRightPointIndex],
                    heightDataNormalized[rightPointIndex]
                );

                colors[currentTriangleCount + 0] = color1;
                colors[currentTriangleCount + 1] = color1;
                colors[currentTriangleCount + 2] = color1;

                colors[currentTriangleCount + 3] = color2;
                colors[currentTriangleCount + 4] = color2;
                colors[currentTriangleCount + 5] = color2;

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

                    float noiseForThisOct = Remap(Mathf.PerlinNoise(xPosAdj, zPosAdj), -1, 1, 0, 1);
                    noiseForAllOct += noiseForThisOct * amplitude;

                    frequency *= mNoiseData.lacunarity;
                    amplitude *= mNoiseData.presistance;
                }

                float normalizedNoise = Remap(noiseForAllOct, 0, sMaxNoiseHeight, 0, 1);
                ChunkManager.allpoints.Add(normalizedNoise);
                heightDataNormalized[currentIndex] = normalizedNoise;
            }
        }
    }
    void MakeColorData()
    {

    }
    void HeightDataCurveAdj()
    {

    }
    static void MakeStaticColors()
    {
        sColorBank = new Color[8];
        sColorBank[0] = new Color( 0.12f,0.11f,0.79f ); // deep water
        sColorBank[1] = new Color(0.25f,0.42f,0.74f); // shallow water
        sColorBank[2] =  new  Color(0.83f,0.88f,0.13f); // sand
        sColorBank[3] = new Color(0.22f,0.88f,0.13f); // shallow grass
        sColorBank[4] = new Color(0.14f,0.68f,0.07f); // deep grass

        sColorBank[5] = new Color(0.68f,0.31f,0.07f); // shallow mountian
        sColorBank[6] = new Color(0.49f,0.24f,0.06f); // deep mountain
        sColorBank[7] = new Color(0.8f, 0.8f, 0.9f); // snow
    }
    Color ColorFromHeight(float h1, float h2, float h3)
    {
        float midPoint = (h1 + h2 + h3) / 3;

        float mean = 0.7325492f;
        float stdDevi = 0.04852225f;
        float z10 = -1.282f;
        float z20 = -0.842f;
        float z25 = -0.674f;
        // float z30 = -0.524f;
        // float z40 = -0.253f;
        float z50 = 0f;
        float z60 = +0.253f;
        float z65 = +0.385f;
        float z70 = +0.524f;
        float z80 = +0.842f;
        float z90 = +1.282f;
        float z95 = +1.645f;
        Color color;
        float currZ = (midPoint - mean) / stdDevi;

        if(currZ < z10)
            color = sColorBank[0];
        else if(currZ < z20)
            color = sColorBank[1];
        else if(currZ < z25)
            color = sColorBank[2];
        else if(currZ < z50)
            color = sColorBank[3];
        else if(currZ < z65)
            color = sColorBank[4];
        else if(currZ < z80)
            color = sColorBank[5];
        else if(currZ < z95)
            color = sColorBank[6];
        else
            color = sColorBank[7];

        return color;

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
/*
p = 0.00 => z = -∞

p = 0.05 => z = -1.645

p = 0.10 => z = -1.282
p = 0.15 => z = -1.036

p = 0.20 => z = -0.842
p = 0.25 => z = -0.674

p = 0.30 => z = -0.524
p = 0.35 => z = -0.385

p = 0.40 => z = -0.253
p = 0.45 => z = -0.126

p = 0.50 => z = -0

p = 0.55 => z = +0.126
p = 0.60 => z = +0.253

p = 0.65 => z = +0.385
p = 0.70 => z = +0.524

p = 0.75 => z = +0.674
p = 0.80 => z = +0.842

p = 0.85 => z = +1.036
p = 0.90 => z = +1.282

p = 0.95 => z = +1.645

p = 1.00 => z = +∞
*/