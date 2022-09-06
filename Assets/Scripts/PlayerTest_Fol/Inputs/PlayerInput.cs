using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerInputKeySetting keySetting;
    public float xAxis, yAxis;
    public bool jump, running;


    void Update()
    {
        xAxis = boolToInt(Input.GetKey(keySetting.left)) - boolToInt(Input.GetKey(keySetting.right));
        yAxis = boolToInt(Input.GetKey(keySetting.forward)) - boolToInt(Input.GetKey(keySetting.backward));
        jump = Input.GetKey(keySetting.jump);
        if(Input.GetKeyDown(keySetting.run))
            running = !running;
    }

    static int boolToInt(bool input) => System.Convert.ToInt32(input);
}