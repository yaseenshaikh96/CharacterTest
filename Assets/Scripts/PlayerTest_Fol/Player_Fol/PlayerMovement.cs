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
        PlayerState.Update();
        UpdateVariables();
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


    void Move(float magnitude)
    {

        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;
        Vector3 playerForward = characterController.transform.forward;
        Vector3 playerSideway = characterController.transform.right;
        Vector3 newPlayerDir = (playerForward * zAxis) + (playerSideway * xAxis);
        newPlayerDir = newPlayerDir.normalized;

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

    //--------------------------------------------------------------------------------------------------------------------------//

    Vector3 cameraOldPos, cameraNewPos;
    bool IsCameraMoving()
    {
        return cameraNewPos != cameraOldPos;
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
        private const float maxSpeed = 100f / 1000; // magnitude if  1ms, 1fps
        private float currentSpeed = 0;
        private const float speedInc = maxSpeed; // speedInc over one second
        bool isMoving = false;

        public override void Action()
        {
            if (playerMovement.IsCameraMoving())
                playerMovement.RotateFromCamera();

            if (playerMovement.IsTryingMoving())
                currentSpeed += speedInc * Time.deltaTime;
            else
                currentSpeed -= speedInc * Time.deltaTime;

            currentSpeed = Mathf.Clamp(currentSpeed, 0, maxSpeed);

            if (currentSpeed == 0)
            {
                isMoving = false;
                return;
            }
            Debug.Log("maxspeed: " + maxSpeed + ", inc: " + speedInc * Time.deltaTime + ", current: " + currentSpeed);
            playerMovement.Move(currentSpeed);
        }
        public override void Initialize()
        {
            isMoving = true;
        }
        public override void Terminate()
        {
            currentSpeed = 0;
        }
        public override void CheckForSwitch()
        {
            if (playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);
            if (!isMoving)
                SwitchState(PlayerStateE.idle);
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