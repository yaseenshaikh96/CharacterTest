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
    public float GetHeight()
    {
        return (float)Phi(ChunkLayer.zValues[zValueIndex]);
    }
    public static double Phi(double x)
    {
        // constants
        double a1 = 0.254829592;
        double a2 = -0.284496736;
        double a3 = 1.421413741;
        double a4 = -1.453152027;
        double a5 = 1.061405429;
        double p = 0.3275911;

        // Save the sign of x
        int sign = 1;
        if (x < 0)
            sign = -1;
        x = System.Math.Abs(x) / System.Math.Sqrt(2.0);

        // A&S formula 7.1.26
        double t = 1.0 / (1.0 + p*x);
        double y = 1.0 - (((((a5*t + a4)*t) + a3)*t + a2)*t + a1)*t * System.Math.Exp(-x*x);

        return 0.5 * (1.0 + sign*y);
    }
}