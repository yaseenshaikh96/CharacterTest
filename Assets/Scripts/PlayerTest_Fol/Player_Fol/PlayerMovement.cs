using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;

    void Start()
    {
        characterController.enabled = true;
    }

    // Update is called once per frame
    void Update()
    {
        float speed = 0.04f;
        float hori = Input.GetAxis("Horizontal");
        float vert = Input.GetAxis("Vertical");

        Vector3 playerDir = new Vector3(hori, 0, vert).normalized;
        Debug.Log(playerDir);

        characterController.Move(playerDir * speed);
    }
}
