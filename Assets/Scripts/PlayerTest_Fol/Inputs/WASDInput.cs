using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WASDInput : MonoBehaviour
{
    int xAxis, yAxis;

    void Update()
    {
        xAxis = System.Convert.ToInt32( Input.GetKey(KeyCode.D)) - System.Convert.ToInt32( Input.GetKey(KeyCode.A));
        yAxis = System.Convert.ToInt32( Input.GetKey(KeyCode.W)) - System.Convert.ToInt32( Input.GetKey(KeyCode.S));

        Debug.Log(
            "WASD Class \n" +
            "xAxis: " + xAxis + "\n" +
            "yAxis: " + yAxis
        );

    }
}
