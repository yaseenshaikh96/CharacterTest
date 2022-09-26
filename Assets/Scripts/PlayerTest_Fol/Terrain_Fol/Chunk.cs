using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chunk
{
    public static float sChunkSize { get; private set; }
    public static int sPointsPerChunk { get; private set; }
    public static Material sMeshMaterial { get; private set; }
    public static bool sIsSmoothMesh { get; private set; }
    private static NoiseData mNoiseData;

    public Vector3 chunkPos { get; private set; } // unique ID
    //---------------------------------------------------------------------------------//
    Vector3[] vertexPositions;
    Vector3[] vertices;
    int[] triangles;
    private GameObject meshGO;
    float[] heightData; // height of each vertex

    //---------------------------------------------------------------------------------//

    public static void Init
    (
        int pointsPerChunk, float chunkSize, bool isSmoothMesh, Material meshMaterial,
        NoiseData noiseData
    )
    {
        sPointsPerChunk = pointsPerChunk;
        sChunkSize = chunkSize;
        sIsSmoothMesh = isSmoothMesh;
        sMeshMaterial = meshMaterial;
        mNoiseData = noiseData;



        verticesPosIndexCount = sPointsPerChunk * sPointsPerChunk;
        triangleIndexCount = 6 * (sPointsPerChunk - 1) * (sPointsPerChunk - 1);
    }

    public Chunk(Vector3 worldPos)
    {
        chunkPos = worldPos;
        heightData = new float[verticesPosIndexCount];
        vertexPositions = new Vector3[verticesPosIndexCount];
        triangles = new int[triangleIndexCount];

        if (sIsSmoothMesh)
            vertices = new Vector3[verticesPosIndexCount];
        else
            vertices = new Vector3[triangleIndexCount];
    }

    public void Delete()
    {
        UnityEngine.GameObject.Destroy(meshGO);
    }
    ~Chunk()
    {
        Delete();
    }
    static int verticesPosIndexCount, triangleIndexCount;
    public void Generate()
    {
        MakeHeightData();

        for (int xIndex = 0; xIndex < sPointsPerChunk; xIndex++)
        {
            for (int zIndex = 0; zIndex < sPointsPerChunk; zIndex++)
            {
                int currentIndex = (xIndex * sPointsPerChunk) + zIndex;



                float xPos = ((float)(xIndex) / (sPointsPerChunk - 1)) * sChunkSize;
                float zPos = ((float)(zIndex) / (sPointsPerChunk - 1)) * sChunkSize;
                float yPos = heightData[currentIndex];

                vertexPositions[currentIndex] = (new Vector3(xPos, yPos, zPos)) + chunkPos;
            }
        }

        if (sIsSmoothMesh)
            SmoothMesh();
        else
            FlatMesh();
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
        for (int index = 0; index < verticesPosIndexCount; index++)
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
                xPos += chunkPos.x; // worldPos
                zPos += chunkPos.z; // worldPos

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
                // normalize height
                // Debug.Log("CurrentHeight: " + noiseForAllOct);
                heightData[currentIndex] = noiseForAllOct;
            }
        }
    }

    public void MakeGameObject()
    {
        Mesh mesh = new Mesh();
        mesh.SetVertices(vertices);
        mesh.triangles = triangles;

        mesh.RecalculateNormals();
        // mesh.SetNormals(CalculateNormals());


        meshGO = new GameObject($"Chunk {chunkPos.x} {chunkPos.z}");
        meshGO.AddComponent<MeshFilter>().mesh = mesh;
        meshGO.AddComponent<MeshRenderer>().material = sMeshMaterial;
    }

    //U = p2 - p1 and the vector V = p3 - p1 then the normal N = U X V
    Vector3[] CalculateNormals()
    {
        /*
            normals are assigned to vertex.
            so make it so that every triangle has unique points which it doesnt share with anyone
            basically double the vertices
            mesh wiil be made up of disjointed triangles
        
        */
        Vector3[] normals = new Vector3[triangleIndexCount / 3];

        for (int index = 0; index < normals.Length; index++)
        {
            Vector3 point1 = vertexPositions[(int)triangles[(index * 3) + 0]];
            Vector3 point2 = vertexPositions[(int)triangles[(index * 3) + 1]];
            Vector3 point3 = vertexPositions[(int)triangles[(index * 3) + 2]];

            Vector3 U = point2 - point1;
            Vector3 V = point3 - point1;

            normals[index] = Vector3.Cross(U, V);

        }

        return normals;
    }

}