using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;

    void Start()
    {
        characterController.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0.1f;
        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;

        Vector3 playerDir = new Vector3(xAxis, 0, zAxis).normalized;

        if (characterController.isGrounded)
            characterController.Move(playerDir * speed);
        else
            characterController.Move(Vector3.down * 0.04f);

    }
}
