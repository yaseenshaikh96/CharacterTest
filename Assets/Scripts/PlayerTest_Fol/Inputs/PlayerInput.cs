using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerInputKeySetting keySetting;
    //------------------------------------------------------------//
    public float xAxis, zAxis;
    public bool jump, running;

    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        zAxis = Input.GetAxisRaw("Vertical");
        jump = Input.GetKeyDown(keySetting.jump);
        running = Input.GetKey(keySetting.run);
    }
}
/*
    float SmoothValue(float value, float controlValue)
    {
       mSpeedIncTimeAdj = mSpeedInc * Time.deltaTime;
        
        if(controlValue != 0){
            value += mSpeedIncTimeAdj * Mathf.Sign(controlValue);
        }
        else{
            if(Mathf.Abs(value) < 0.05f)
                return 0;
            value -= 3 * mSpeedIncTimeAdj * Mathf.Sign(value);
        }
        return Mathf.Clamp(value, -1, 1);
    }
*/