using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CreateGrid : MonoBehaviour
{
    [SerializeField] private Material material;
    [SerializeField] private Vector2 size;

    void Start()
    {
        GameObject obj;
        for(int x = 0; x<size.x; x++) {
            for(int y=0; y<size.y; y++) {
                obj = GameObject.CreatePrimitive(PrimitiveType.Cube);
                obj.transform.position = new Vector3(2*x, 0, 2*y);
                var renderer = obj.GetComponent<MeshRenderer>();
                renderer.material = material;
            }
        }
    }

}
