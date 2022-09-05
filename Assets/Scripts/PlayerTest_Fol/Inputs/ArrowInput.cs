using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ArrowInput : MonoBehaviour
{
    int xAxis, yAxis;

    void Update()
    {
        xAxis = System.Convert.ToInt32( Input.GetKey(KeyCode.RightArrow)) - System.Convert.ToInt32( Input.GetKey(KeyCode.LeftArrow));
        yAxis = System.Convert.ToInt32( Input.GetKey(KeyCode.UpArrow)) - System.Convert.ToInt32( Input.GetKey(KeyCode.DownArrow));

        Debug.Log(
            "Arrow Class \n" +
            "xAxis: " + xAxis + "\n" +
            "yAxis: " + yAxis
        );
    
    }
}
