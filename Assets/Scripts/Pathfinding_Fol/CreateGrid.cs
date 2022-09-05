using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[ExecuteInEditMode]
public class CreateGrid : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Vector2 size;
    [SerializeField] private GameObject plane, startGO, DestGO;

    [SerializeField] private Vector2 startPos, DestPos;
    
    void Start()
    {
        float height = 0.2f;
        plane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        plane.transform.localScale = new Vector3(size.x/10, 1, size.y/10);
        plane.GetComponent<MeshRenderer>().material = material;

        startGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        DestGO = GameObject.CreatePrimitive(PrimitiveType.Capsule);
        startGO.transform.position = new Vector3(startPos.x + 0.5f, height, startPos.y + 0.5f);
        startGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        DestGO.transform.position = new Vector3(DestPos.x + 0.5f, height, DestPos.y + 0.5f);
        DestGO.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
    }
    void OnDrawGizmos() {
        float h = 0.01f;
        Gizmos.color = Color.black;
        Handles.color = Color.black;
        for(int y=(int)(-size.y/2); y<=(int)(size.y/2); y++) {
            Handles.DrawLine(new Vector3(-size.x/2, h, y), new Vector3(size.x/2, h, y));
        }
        for(int x=(int)(-size.x/2); x<=(int)(size.x/2); x++) {
            Gizmos.DrawLine(new Vector3(x, h, -size.y/2), new Vector3(x, h, +size.y/2));
        }
    }
}
