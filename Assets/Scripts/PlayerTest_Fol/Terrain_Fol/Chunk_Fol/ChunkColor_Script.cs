using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChunkColor_Script : MonoBehaviour
{
    void Start()
    {

    }

}
public class ChunkLayers
{
    public float transitionGap { get; private set; }
    public ChunkLayer[] chunkLayers;

    public ChunkLayers(int layerCount, float _transitionGap, float[] layerValue, float[] layerSize, Color[] layerColor)
    {
        chunkLayers = new ChunkLayer[layerCount];
        transitionGap = _transitionGap;

        float previousEnd = 0;
        for (int i = 0; i < layerCount; i++)
        {
            chunkLayers[i] = new ChunkLayer(layerValue[i], previousEnd, previousEnd + layerSize[i], layerColor[i]);
            previousEnd += layerSize[i];
        }
    }


    public void MakeCurve()
    {
        /*
            two types of ket frame.
        */
        Keyframe keyframe0 = new Keyframe(0, 0);
        Keyframe keyframe1 = new Keyframe(chunkLayers[0].end, chunkLayers[0].value);

        List<Keyframe> keyframes = new List<Keyframe>(20);
        keyframes.Add(keyframe0);
        keyframes.Add(keyframe1);

        for (int i = 0; i < chunkLayers.GetLength(0) - 1; i++)
        {
            //make s shape for transition

            //make line for layer
            Keyframe kf1 = new Keyframe();
        }

        AnimationCurve animationCurve = new AnimationCurve();
    }

    public class ChunkLayer
    {
        public Color color;
        public float start; // percentile 
        public float end; // end
        public float value;
        public ChunkLayer(float _value, float _start, float _end, Color _color)
        {
            value = _value;
            start = _start;
            end = _end;
            color = _color;
        }
    }
}