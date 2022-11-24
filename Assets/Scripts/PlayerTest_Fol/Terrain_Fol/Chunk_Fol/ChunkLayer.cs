using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkLayer 
{
    public static float[] zValues;
    public static float mean, stdDevi;
    //----------------------------------------------//
    public int zValueIndex;
    public Color color;

    public static void Init(float[] _zValues, float _mean, float _stdDevi)
    {
        zValues = _zValues;
        mean = _mean;
        stdDevi = _stdDevi;
    }
    public ChunkLayer(int zVal, Color col)
    {
        zValueIndex = zVal;
        color = col;
    }
}