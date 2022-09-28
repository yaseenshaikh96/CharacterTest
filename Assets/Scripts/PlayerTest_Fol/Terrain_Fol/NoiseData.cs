using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.Serializable]
public class NoiseData
{
    public int seed;
    [Range(1, 300)]public float scale;
    [Range(1, 6)] public int octave;
    [Range(0, 5)]public float lacunarity;
    [Range(0, 1)] public float presistance;
    public Vector2 offset;

    public NoiseData(float _scale, int _octave, float _lacunarity, float _presistance, Vector2 _offset)
    {
        scale = _scale;
        octave = _octave;
        lacunarity = _lacunarity;
        presistance = _presistance;
        offset = _offset;
    }
}
