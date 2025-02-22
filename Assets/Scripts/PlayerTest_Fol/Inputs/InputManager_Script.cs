using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputManager_Script : MonoBehaviour
{
    [SerializeField] private WASDInput wASDInput;
    [SerializeField] private ArrowInput arrowInput;
    [SerializeField] private PlayerInput playerInput;

    void Start()
    {
        wASDInput.enabled = false;
        arrowInput.enabled = false;
        playerInput.enabled = true;
    }

    void Update()
    {
        // bool ChangeInputKey = Input.GetKeyDown(KeyCode.G);
        // if(ChangeInputKey) {
        //     wASDInput.enabled = arrowInput.enabled;
        //     arrowInput.enabled = !arrowInput.enabled;
        // }        

        // Debug.Log(
        //     "G pressed: " + ChangeInputKey + "\n" +
        //     "wasd     : " + wASDInput.enabled + "\n" +
        //     "arrow    : " + arrowInput.enabled);

    }
}
