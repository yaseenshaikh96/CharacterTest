using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TimeVariant
{
    public readonly float maxValue, minValue, timeWhenMax;
    public float currentValue;
    float mMultipleFactor;
    public float multipleFactorNormalized { get; private set; }

    public TimeVariant(float min, float max, float secondsTillMax)
    {
        currentValue = min;
        maxValue = max;
        minValue = min;
        timeWhenMax = secondsTillMax;
        mMultipleFactor = 0;
        multipleFactorNormalized = 0;
    }

    public void Increment(float incrementFactor = 1)
    {
        mMultipleFactor += incrementFactor * Time.deltaTime;
    }
    public void Decrement(float decrementFactor = 1)
    {
        mMultipleFactor -= decrementFactor * Time.deltaTime;
    }
    public void Update()
    {
        mMultipleFactor = Mathf.Clamp(mMultipleFactor, 0, timeWhenMax);
        multipleFactorNormalized = Remap(mMultipleFactor, 0, timeWhenMax, 0, 1);
        currentValue = Remap(multipleFactorNormalized, 0, 1, minValue, maxValue);
    }
    public void Reset()
    {
        currentValue = minValue;
        mMultipleFactor = 0;
        multipleFactorNormalized = 0;
    }

    static float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
        return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
    }
}
