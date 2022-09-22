using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerInputKeySetting keySetting;
    private float mXAxis, mZAxis, mMouseX, mMouseY;
    //------------------------------------------------------------//
    public float xAxis, zAxis, mouseX, mouseY;
    public bool jump, running;

    void Update()
    {
        xAxis = Input.GetAxisRaw("Horizontal");
        zAxis = Input.GetAxisRaw("Vertical");

        jump = Input.GetKey(keySetting.jump);
        if (Input.GetKeyDown(keySetting.run))
            running = !running;

        // Debug.Log("mXaxis: " + mXAxis + ", xAxis: " + xAxis + " : mZaxis: " + mZAxis + ", zAxis: " + zAxis);
    }

    public float SmoothValue(float value, float min, float max, float increment)
    {
        value += increment;
        return Mathf.Clamp(value, min, max);
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