using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum MeshType
{
    flat, smooth
}
public class Chunk
{
    public static float sLayerEndDeepWater;
    public static float sLayerEndShallowWater;
    public static float sLayerEndSand;
    public static float sLayerEndLightGrass;
    public static float sLayerEndDarkGrass;
    public static float sLayerEndLightMountain;
    public static float sLayerEndDarkMountain;
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
    static float pointHeightMean, pointHeightStdDevi;

    public static ChunkLayer[] layers;
    //---------------------------------------------------------------------------------//
    public bool heightDataLoaded { get; private set; }
    public bool gameObjectMade { get; private set; }
    public bool hasCollider { get; private set; }
    public Vector3 mWorldPos { get; private set; } // unique ID
    public Vector3 mWorldPosCentered { get; private set; } 
    public Vector3 mChunkPos { get; private set; }
    public float timeWhenCreated;
    //---------------------------------------------------------------------------------//
    AnimationCurve mHeightCurve;
    List<Tree> spawnableGOs;
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
        float heightMultiplier, AnimationCurve heightCurve,
        ChunkLayer[] _layers
    )
    {

        Random.InitState(noiseData.seed);

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

        sVerticesPosIndexCount = sPointsPerChunk * sPointsPerChunk;
        sTriangleIndexCount = 6 * (sPointsPerChunk - 1) * (sPointsPerChunk - 1);

        MakeStaticColors();
        CalcMeanAndStdDevi();
        CalcLayerValues();
        
        layers = _layers;
    }
    public Chunk(Vector3 chunkPos)
    {
        timeWhenCreated = Time.time;
        mWorldPos = chunkPos * sChunkSize;
        mWorldPosCentered = new Vector3(mWorldPos.x + (sChunkSize/2), 0 ,mWorldPos.z + (sChunkSize/2));
        mChunkPos = chunkPos;

        mHeightCurve = new AnimationCurve(sHeightCurve.keys);

        heightDataLoaded = false;
        gameObjectMade = false;
        hasCollider = false;

    }
    public void Delete()
    {
        for (int i = spawnableGOs.Count - 1; i > -1; i--)
        {
            UnityEngine.GameObject.Destroy(spawnableGOs[i].mTreeGO);
        }

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
        spawnableGOs = new List<Tree>();

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
                float yPos = mHeightCurve.Evaluate(heightDataNormalized[currentIndex]) * sHeightMultiplier; // heightCure.Evaluate();

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
        hasCollider = true;

        meshGO.layer = sGroundLayer.value >> 5;

        MakeWaterMesh();
        SpawnSpawnable();
        gameObjectMade = true;

    }
    //-------------------------------------------------------------------------------//
    public void AddCollider()
    {
        if(!hasCollider)
        {
            mMeshCollider.enabled = true; 
            foreach(var spawns in spawnableGOs)
            {
                spawns.AddCollider();
            }
            hasCollider = true;
        }
    }
    public void RemoveCollider()
    {
        if(hasCollider)
        {
            mMeshCollider.enabled = false; 
            foreach(var spawns in spawnableGOs)
            {
                spawns.RemoveCollider();
            }
            hasCollider = false;
        }
    }
    void SpawnSpawnable()
    {
        Random.InitState(mNoiseData.seed);
        for (int i = 0; i < spawnablePoints.Length; i++)
        {
            if (spawnablePoints[i] && Random.value < 0.05f)
            {
                Tree tree = new Tree(vertexPositions[i], meshGO);
                spawnableGOs.Add(tree);
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
                // Debug.Log("currentpoint.y: " + currentPoint.y + 
                //     ", sand: " + layerEndSand * sHeightMultiplier +
                //     ", darkGrass: " + layerEndDarkGrass * sHeightMultiplier);
                if (
                    deviation < 2f &&
                    currentPoint.y > sLayerEndSand * sHeightMultiplier &&
                    currentPoint.y < sLayerEndDarkGrass * sHeightMultiplier
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
                deviation += (values[i] - avg) * (values[i] - avg);

            return Mathf.Sqrt(deviation / values.Length);
        }
    }
    void MakeWaterMesh()
    {
        waterParent = new GameObject("waterParent");
        waterParent.transform.parent = meshGO.transform;

        waterParent.transform.position = mWorldPos;
        GameObject waterGO = UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Quad);
        waterGO.GetComponent<Collider>().enabled = false;
        waterGO.transform.Rotate(Vector3.right * 90);
        waterGO.transform.localScale = Vector3.one * sChunkSize;
        waterGO.transform.parent = waterParent.transform;
        waterGO.transform.localPosition = new Vector3(
            sChunkSize / 2,
            sHeightMultiplier * ((sLayerEndShallowWater + sLayerEndSand) / 2),
            sChunkSize / 2
        );
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

                colors[currentTriangleCount + 3] = color1; // color2
                colors[currentTriangleCount + 4] = color1; // color2
                colors[currentTriangleCount + 5] = color1; // color2

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

        Vector2[] octaveOffset = GetOctaveOffsetPoint();

        for (int xIndex = 0; xIndex < sPointsPerChunk; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk; zIndex++)
            {
                int currentIndex = (xIndex * sPointsPerChunk) + zIndex;

                float xPos = ((float)xIndex / (sPointsPerChunk - 1)) * sChunkSize;
                float zPos = ((float)zIndex / (sPointsPerChunk - 1)) * sChunkSize;
                xPos += mWorldPos.x; // worldPos
                zPos += mWorldPos.z; // worldPos
                float noise = getNoiseValue(xPos, zPos, octaveOffset);
                
                // ChunkManager.allpoints.Add(normalizedNoise);
                heightDataNormalized[currentIndex] = noise;
            }
        }
    }

    static void MakeStaticColors()
    {
        sColorBank = new Color[8];
        sColorBank[0] = new Color(0.12f, 0.11f, 0.79f); // deep water
        sColorBank[1] = new Color(0.25f, 0.42f, 0.74f); // shallow water
        sColorBank[2] = new Color(0.83f, 0.88f, 0.13f); // sand
        sColorBank[3] = new Color(0.22f, 0.88f, 0.13f); // shallow grass
        sColorBank[4] = new Color(0.14f, 0.68f, 0.07f); // deep grass

        sColorBank[5] = new Color(0.68f, 0.31f, 0.07f); // shallow mountian
        sColorBank[6] = new Color(0.49f, 0.24f, 0.06f); // deep mountain
        sColorBank[7] = new Color(0.8f, 0.8f, 0.9f); // snow
    }
    Color ColorFromHeight(float h1, float h2, float h3)
    {
        float midPoint = (h1 + h2 + h3) / 3;
        float currZ = (midPoint - pointHeightMean) / pointHeightStdDevi;

        for(int i=0; i<layers.Length; i++)
        {
            if(currZ < ChunkLayer.zValues[layers[i].zValueIndex])
                return layers[i].color;
        }
        return layers[layers.Length - 1].color;
    }

    //----------------------------------------------------------------------------------------//

    static void CalcMeanAndStdDevi()
    {
        pointHeightMean = 0.7325492f;
        pointHeightStdDevi = 0.04852225f;
    }

    static void CalcLayerValues()
    {
        float[] zValues = new float[21] {
            -10f   , -1.645f,   // 00: 0.00, 01: 0.05
            -1.282f, -1.036f,   // 02: 0.10, 03: 0.15
            -0.842f, -0.674f,   // 04: 0.20, 05: 0.25
            -0.524f, -0.385f,   // 06: 0.30, 07: 0.35
            -0.253f, -0.126f,   // 08: 0.40, 09: 0.45
            0,                  // 10: 0.50
            0.126f, 0.253f,     // 12: 0.55, 13: 0.60
            0.385f, 0.524f,     // 14: 0.65, 15: 0.70
            0.674f, 0.842f,     // 16: 0.75, 17: 0.80
            1.036f, 1.282f,     // 18: 0.85, 19: 0.90
            1.645f, 10f         // 20: 0.95, 21: 1.00
        };
        
        ChunkLayer.Init(zValues, pointHeightMean, pointHeightStdDevi);

        // layers = new ChunkLayer[8]
        // {
        //     new ChunkLayer(2, new Color(0.12f, 0.11f, 0.79f)), // deep water
        //     new ChunkLayer(3, new Color(0.25f, 0.42f, 0.74f)), // shallow water
        //     new ChunkLayer(5, new Color(0.83f, 0.88f, 0.13f)), // sand
        //     new ChunkLayer(10, new Color(0.22f, 0.88f, 0.13f)), // shallow grass
        //     new ChunkLayer(14, new Color(0.14f, 0.68f, 0.07f)), // deep grass
        //     new ChunkLayer(17, new Color(0.68f, 0.31f, 0.07f)), // shallow mountian
        //     new ChunkLayer(20, new Color(0.49f, 0.24f, 0.06f)), // deep mountain
        //     new ChunkLayer(21, new Color(0.8f, 0.8f, 0.9f)) // snow                 
        // };

        sLayerEndDeepWater = sHeightCurve.Evaluate(0.670443f);
        sLayerEndShallowWater = sHeightCurve.Evaluate(0.682520f);
        sLayerEndSand = sHeightCurve.Evaluate(0.700290f);
        sLayerEndLightGrass = sHeightCurve.Evaluate(0.733372f);
        sLayerEndDarkGrass = sHeightCurve.Evaluate(0.752275f);
        sLayerEndLightMountain = sHeightCurve.Evaluate(0.774707f);
        sLayerEndDarkMountain = sHeightCurve.Evaluate(0.814128f);
    }
    Vector2[] GetOctaveOffsetPoint()
    {
        System.Random prng = new System.Random(mNoiseData.seed);
        Vector2[] octaveOffsets = new Vector2[mNoiseData.octave];
        for (int i = 0; i < mNoiseData.octave; i++)
        {
            octaveOffsets[i].x = prng.Next(-10000, 10000) + mNoiseData.offset.x;
            octaveOffsets[i].y = prng.Next(-10000, 10000) + mNoiseData.offset.y;
        }
        return octaveOffsets;
    }

    float getNoiseValue(float xWorldPos, float zWorldPos, Vector2[] octaveOffsets)
    {
        float frequency = 1;
        float amplitude = 1;

        float noiseForAllOct = 0;
        for (int octIndex = 0; octIndex < mNoiseData.octave; octIndex++)
        {
            float xPosAdj = (xWorldPos + octaveOffsets[octIndex].x) / mNoiseData.scale * frequency;
            float zPosAdj = (zWorldPos + octaveOffsets[octIndex].y) / mNoiseData.scale * frequency;

            float noiseForThisOct = Remap(Mathf.PerlinNoise(xPosAdj, zPosAdj), -1, 1, 0, 1);
            noiseForAllOct += noiseForThisOct * amplitude;

            frequency *= mNoiseData.lacunarity;
            amplitude *= mNoiseData.presistance;
        }
        return Remap(noiseForAllOct, 0, sMaxNoiseHeight, 0, 1);
    }

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