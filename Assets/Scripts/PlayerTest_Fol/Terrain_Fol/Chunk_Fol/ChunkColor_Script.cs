using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkColor_Script : MonoBehaviour
{
    ChunkLayers chunkLayers;
    void Start()
    {
        float[] sizes = { 0.5f, 0.5f};
        float[] values = { 0.1f, 0.9f };
        Color[] colors = { Color.red, Color.black };

        chunkLayers = new ChunkLayers(2, sizes, values, colors);

        Draw();
    }
    void Draw()
    {
        float step = 0.01f;
        for (float i = 0; i < 1; i += step)
        {
            float value = chunkLayers.bezierurve.Evaluate(i);
        
            GameObject go= UnityEngine.GameObject.CreatePrimitive(UnityEngine.PrimitiveType.Cube);
            Debug.Log(" i: " + i + ", v: " + value);
            go.transform.position = (new Vector3(i, value, 0)) * 100;
        }
    }
}

public class ChunkLayers
{
    public static float transitionGab = 0.05f;
    public Bezierurve bezierurve;
    ChunkLayer[] chunkLayers;

    public ChunkLayers(int layerCount, float[] layerSizes, float[] layerValues, Color[] layerColors)
    {
        chunkLayers = new ChunkLayer[layerCount];
        float previousEnd = 0;
        for (int i = 0; i < layerCount; i++)
        {
            chunkLayers[i] = new ChunkLayer(layerValues[i], previousEnd, previousEnd + layerSizes[i], layerColors[i]);
            previousEnd += layerSizes[i];
        }
        MakeCurve();
    }

    void MakeCurve()
    {
        Vector2 point1 = new Vector2(chunkLayers[0].start, chunkLayers[0].value);
        Vector2 point2 = new Vector2((chunkLayers[0].start + chunkLayers[0].end - transitionGab) / 2, chunkLayers[0].value);
        Vector2 point3 = new Vector2(chunkLayers[0].end - transitionGab, chunkLayers[0].value);

        List<Vector2> points = new List<Vector2>(40);
        points.Add(point1);
        points.Add(point2);
        points.Add(point3);
        for (int i = 1; i < chunkLayers.Length; i++)
        {
            // three vertical points
            float timeV = chunkLayers[i - 1].end - (transitionGab / 2);
            Vector2 pointV1 = new Vector2(timeV, chunkLayers[i - 1].value);
            Vector2 pointV2 = new Vector2(timeV, (chunkLayers[i - 1].value + chunkLayers[i].value) / 2);
            Vector2 pointV3 = new Vector2(timeV, chunkLayers[i].value);

            // three horizontal points
            Vector2 pointH1 = new Vector2(chunkLayers[i].start, chunkLayers[i].value);
            Vector2 pointH2 = new Vector2((chunkLayers[i].start + chunkLayers[i].end - transitionGab) / 2, chunkLayers[i].value);
            Vector2 pointH3 = new Vector2(chunkLayers[i].end - transitionGab, chunkLayers[i].value);

            points.Add(pointV1);
            points.Add(pointV2);
            points.Add(pointV3);
            points.Add(pointH1);
            points.Add(pointH2);
            points.Add(pointH3);
        }

        bezierurve = new Bezierurve(points.ToArray());
    }
}

public class ChunkLayer
{
    public Color color;
    public float value;
    public float start, end;
    public ChunkLayer(float _value, float _start, float _end, Color _color)
    {
        value = _value;
        start = _start;
        end = _end;
        color = _color;
    }
}

public class Bezierurve
{ // x = time, y = value
    public Vector2[] controlPoints;
    public Bezierurve(params Vector2[] _controlPoints)
    {
        controlPoints = _controlPoints;
    }

    public float Evaluate(float t)
    {
        Vector2 point = DeCasteljau(controlPoints);

        return point.y;

        Vector2 DeCasteljau(params Vector2[] points)
        {
            if (points.Length <= 1)
                return points[0];

            Vector2[] newPoints = new Vector2[points.Length - 1];
            for (int i = 0; i < points.Length - 1; i++)
                newPoints[i] = Vector2.Lerp(points[i], points[i + 1], t);

            return DeCasteljau(newPoints);
        }

    }
}