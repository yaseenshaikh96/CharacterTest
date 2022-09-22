using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerInput : MonoBehaviour
{
    [SerializeField] private PlayerInputKeySetting keySetting;
    private float mXAxis, mZAxis;
    public float xAxis, zAxis;
    public bool jump, running;

    private const float mSpeedInc = 0.02f;

    void Update()
    {
        mXAxis = Input.GetAxisRaw("Horizontal");
        mZAxis = Input.GetAxisRaw("Vertical");
        xAxis = IncrementSpeed(xAxis, mXAxis);
        zAxis = IncrementSpeed(zAxis, mZAxis);

        jump = Input.GetKey(keySetting.jump);
        if (Input.GetKeyDown(keySetting.run))
            running = !running;

        Debug.Log("mXaxis: " + mXAxis + ", xAxis: " + xAxis + " : mZaxis: " + mZAxis + ", zAxis: " + zAxis);
    }


    float IncrementSpeed(float value, float controlValue)
    {
        if(controlValue > 0)
        {
            value += mSpeedInc;
        }
        else if(controlValue < 0)
        {
            value -= mSpeedInc;
        }else{
            if(Mathf.Abs(value) < 0.05f)
                return 0;
            value -= 3 * mSpeedInc * Mathf.Sign(value);
        }
        return Mathf.Clamp(value, -1, 1);
    }

}