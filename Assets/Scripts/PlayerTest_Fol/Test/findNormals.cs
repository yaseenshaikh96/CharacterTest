using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class findNormals : MonoBehaviour
{
    public bool update = false;
    public GameObject point1;
    public GameObject point2;
    public GameObject point3;
    public GameObject crossPoint;

    void Start()
    {
        
    }

    void Update()
    {
        if(update)
        {
            update = false;

            Vector3 pos1 = point1.transform.position;
            Vector3 pos2 = point2.transform.position;
            Vector3 pos3 = point3.transform.position;
        
            Vector3 pos12 = (pos2 - pos1).normalized; //  => b - a
            Vector3 pos23 = (pos3 - pos2).normalized; //  => c - b
        
            Vector3 cross = Vector3.Cross(pos12, pos23);

            Vector3 midpoint = pos1 + pos2 + pos3;
            midpoint /= 3;

            Vector3 newPosForCross = (cross * 10) + midpoint;

            crossPoint.transform.position = newPosForCross;
        }
    }
}
