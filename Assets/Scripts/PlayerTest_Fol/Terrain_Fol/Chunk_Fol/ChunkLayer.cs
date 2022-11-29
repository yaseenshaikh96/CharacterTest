using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class ChunkLayer 
{
    public static float[] zValues;
    public static float mean, stdDevi;
    //----------------------------------------------//
    [Range(0, 100)]public int zValueIndex;
    public Color color;
    public float height {get; private set;}
    public float curveEvaledHeight {get; private set;}

    public static void Init(float _mean, float _stdDevi)
    {
        mean = _mean;
        stdDevi = _stdDevi;
        /*
        zValues = new float[21] {
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
        };*/
        zValues = new float[101] {
            -1000f,
            -2.32635f,
            -2.05375f,
            -1.88079f,
            -1.75069f,
            
            -1.64485f,
            -1.55477f,
            -1.47579f,
            -1.40507f,
            -1.34076f,
            
            -1.28155f,
            -1.22653f,
            -1.17499f,
            -1.12639f,
            -1.08032f,
            
            -1.03643f,
            -0.99446f,
            -0.95417f,
            -0.91537f,
            -0.8779f,
            
            -0.84162f,
            -0.80642f,
            -0.77219f,
            -0.73885f,
            -0.7063f,
            
            -0.67449f,
            -0.64335f,
            -0.61281f,
            -0.58284f,
            -0.55338f,
            
            -0.5244f,
            -0.49585f,
            -0.4677f,
            -0.43991f,
            -0.41246f,
            
            -0.38532f,
            -0.35846f,
            -0.33185f,
            -0.30548f,
            -0.27932f,
            
            -0.25335f,
            -0.22754f,
            -0.20189f,
            -0.17637f,
            -0.15097f,

            -0.12566f,
            -0.10043f,
            -0.07527f,
            -0.05015f,
            -0.02507f,
            
            0,
            
            0.02507f,
            0.05015f,
            0.07527f,
            0.10043f,
            0.12566f,

            0.15097f,            
            0.17637f,
            0.20189f,
            0.22754f,
            0.25335f,

            0.27932f,
            0.30548f,
            0.33185f,
            0.35846f,
            0.38532f,

            0.41246f,
            0.43991f,
            0.4677f,
            0.49585f,
            0.5244f,

            0.55338f,
            0.58284f,
            0.61281f,
            0.64335f,
            0.67449f,

            0.7063f,
            0.73885f,
            0.77219f,
            0.80642f,
            0.84162f,

            0.8779f,
            0.91537f,
            0.95417f,
            0.99446f,
            1.03643f,
        
            1.08032f,
            1.12639f,
            1.17499f,
            1.22653f,
            1.28155f,

            1.34076f,
            1.40507f,
            1.47579f,
            1.55477f,
            1.64485f,

            1.75069f,
            1.88079f,
            2.05375f,
            2.32635f,
            1000f,
        };
    } 
    public ChunkLayer(int zVal, Color col)
    {
        zValueIndex = zVal;
        color = col;
    }
    public void CalcHeight()
    {
        height = (float)Phi(ChunkLayer.zValues[zValueIndex]);
    }
    public void CalcCurveEvaledHeight(AnimationCurve ac)
    {
        curveEvaledHeight = ac.Evaluate(height);
    }
    public static double Phi(double x)
    {
        // credits: https://www.johndcook.com/blog/csharp_phi/
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