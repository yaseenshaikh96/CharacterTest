using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DontChangeTransformOnChild : MonoBehaviour
{
    void Update()
    {
        this.transform.rotation = Quaternion.identity;
    }
}
