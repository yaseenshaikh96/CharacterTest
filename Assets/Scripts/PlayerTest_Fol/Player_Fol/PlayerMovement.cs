using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera cameraMain;

    //--------------------------------------------------------------------------------------------//

    //-----------------------------------------//
    void Start()
    {
        characterController.enabled = true;
        PlayerState.Init(this, characterController);
    }

    void Update()
    {
        UpdateVariables();
        PlayerState.Update();
    }

    //-------------------------------------------------------------------------------------------------------------------------------//

    void UpdateVariables()
    {
        oldPos = newPos;
        newPos = characterController.transform.position;
        cameraOldPos = cameraNewPos;
        cameraNewPos = cameraMain.transform.position;

    }

    //-------------------------------------------------------------------------------------------------------------------------------//

    Vector3 previousDirOfMotion;
    void Move(float magnitude)
    {

        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;
        Vector3 playerForward = characterController.transform.forward;
        Vector3 playerSideway = characterController.transform.right;

        Vector3 newPlayerDir;
        if (xAxis == 0 && zAxis == 0)
            newPlayerDir = previousDirOfMotion;
        else
            newPlayerDir = (playerForward * zAxis) + (playerSideway * xAxis);
        newPlayerDir = newPlayerDir.normalized;

        previousDirOfMotion = newPlayerDir;

            // if d then camera.right
            //if a then camera.left

        if (IsCameraMoving()) 
            RotateFromCamera();

        characterController.Move(newPlayerDir * magnitude);
    }

    const float jumpSpeed = 5f;
    void Jump()
    {
        Vector3 jumpDir = Vector3.up * jumpSpeed;
        characterController.Move(jumpDir * Time.deltaTime);
    }

    //--------------------------------------------------------------------------------------------------------------------------//

    float rotSpeed = 20;
    void RotateFromCamera()
    {
        Transform playerT = characterController.transform;
        Vector3 playerRot = characterController.transform.rotation.eulerAngles;
        Vector3 cameraRot = cameraMain.transform.rotation.eulerAngles;

        Quaternion newRot = Quaternion.Euler(playerRot.x, cameraRot.y, playerRot.z);

        playerT.rotation = Quaternion.Slerp(playerT.rotation, newRot, rotSpeed * Time.deltaTime);

    }
    void RotateWhenMovingSideWays(Vector3 dirOfMotion)
    {
        Quaternion dirOfMotionQuat = Quaternion.Euler(dirOfMotion);
        Transform playerT = characterController.transform;
        Quaternion newRot = Quaternion.Slerp(playerT.rotation, dirOfMotionQuat, rotSpeed * Time.deltaTime);
        playerT.rotation = newRot;
    }

    //--------------------------------------------------------------------------------------------------------------------------//

    Vector3 cameraOldPos, cameraNewPos;
    bool IsCameraMoving()
    {
        return cameraOldPos != cameraNewPos;
        // return cameraOldRot != cameraNewRot;
    }
    bool IsTryingMoving()
    {
        if (playerInput.xAxis != 0 || playerInput.zAxis != 0)
            return true;
        return false;
    }
    bool IsFallingSoft()
    {
        float quaterHeight = characterController.height / 4;
        Vector3 point2 = characterController.transform.TransformPoint(new Vector3(0, 3 * quaterHeight, 0));

        RaycastHit raycastHit;
        if (Physics.SphereCast(point2, characterController.radius, Vector3.down, out raycastHit, 5f, groundLayer))
        {
            float diff = characterController.transform.position.y - raycastHit.point.y;
            if (diff < 0.1f)
            {
                return false;
            }
        }
        return true;
    }

    Vector3 oldPos, newPos;
    bool IsFallingHard()
    {
        return false;
    }

    bool IsJumping()
    {
        return false;
    }

    void ApplyGravity()
    {

    }

    //--------------------------------------------------------------------------------------------------------------------------//
    private enum PlayerStateE
    {
        idle, walking, falling, jumping
    }
    private abstract class PlayerState
    {
        public static PlayerMovement playerMovement { get; private set; }
        private static CharacterController characterController;
        private static PlayerState[] playerStates;
        public static PlayerState currentPlayerState { get; private set; }

        public static void Init(PlayerMovement pm, CharacterController cc)
        {
            playerMovement = pm;
            characterController = cc;

            playerStates = new PlayerState[10];
            playerStates[0] = new PSIdle();
            playerStates[1] = new PSWalking();
            playerStates[2] = new PSFalling();
            playerStates[3] = new PSJumping();


            currentPlayerState = playerStates[0];
        }
        public static void Update()
        {
            currentPlayerState.Action();
            currentPlayerState.CheckForSwitch();
            // Debug.Log("state: " + currentPlayerState);
        }
        protected void SwitchState(PlayerStateE newState)
        {
            currentPlayerState.Terminate();
            currentPlayerState = playerStates[(int)newState];
            currentPlayerState.Initialize();
        }

        public abstract void Action();
        public abstract void CheckForSwitch();
        public abstract void Initialize();
        public abstract void Terminate();


    }


    private class PSIdle : PlayerState
    {
        public override void Action()
        {
        }
        public override void Initialize()
        { }
        public override void Terminate()
        { }
        public override void CheckForSwitch()
        {
            if (playerMovement.IsTryingMoving())
                SwitchState(PlayerStateE.walking);

        }
    }

    private class PSWalking : PlayerState
    {/*
        5 m / s
       1 s = 1000 ms
        if 60fps
            then 1 / 60 = 16.6ms
        if 30 fps
            then 1 / 30 = 33.3ms
        
        speed per ms = 5 / 1000 = 0.005 meters in one ms
        60 fps || 30 fps = speed per ms * time of frame

    */
        private const float maxSpeed = 10f; // 10 m/s
        bool isMoving = false;
        private float speedFactor = 0;
        private float speedFactorTimeAdj = 0;
        private const float timeTillMaxSpeed = 2; // sec

        public override void Action()
        {

            // if (playerMovement.IsCameraMoving())
            //     playerMovement.RotateFromCamera();

            if (playerMovement.IsTryingMoving())
            {
                speedFactor += Time.deltaTime;
            }
            else
                speedFactor -= Time.deltaTime;

            speedFactor = Mathf.Clamp(speedFactor, 0, timeTillMaxSpeed);
            speedFactorTimeAdj = Remap(speedFactor, 0, timeTillMaxSpeed, 0, 1);

            if (speedFactorTimeAdj == 0)
            {
                isMoving = false;
                return;
            }

            // Debug.Log("maxspeed: " + maxSpeed + ", actual: " + maxSpeed * speedFactor * Time.deltaTime + ", speedFactor: " + speedFactor);
            playerMovement.Move(maxSpeed * speedFactorTimeAdj * Time.deltaTime);
        }
        public override void Initialize()
        {
            isMoving = true;
        }
        public override void Terminate()
        {
            speedFactor = 0;
            speedFactorTimeAdj = 0;
        }
        public override void CheckForSwitch()
        {
            // if (playerMovement.IsFallingHard())
            //     SwitchState(PlayerStateE.falling);
            if (!isMoving)
                SwitchState(PlayerStateE.idle);
        }
        float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
        {
            return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
        }
    }

    private class PSFalling : PlayerState
    {
        public override void Action()
        {
        }
        public override void Initialize()
        { }
        public override void Terminate()
        { }
        public override void CheckForSwitch()
        {
        }
    }

    private class PSJumping : PlayerState
    {
        public override void Action()
        {
        }
        public override void Initialize()
        { }
        public override void Terminate()
        { }
        public override void CheckForSwitch()
        {
        }
    }
}