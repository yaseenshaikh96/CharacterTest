using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class PlayerInput
{
    [SerializeField] public Vector3 dir;
    [SerializeField] public bool isJumping;

    public void Update()
    {
        dir.x = Input.GetAxisRaw("Horizontal");
        dir.y = Input.GetAxisRaw("Vertical");
    }
}