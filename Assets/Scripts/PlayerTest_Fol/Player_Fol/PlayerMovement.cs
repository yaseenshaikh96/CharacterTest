using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Camera cameraMain;

    private PlayerState[] playerStates;
    private PlayerState currentPS;
    private PlayerStateE currentPSE;
    //-----------------------------------------//
    Vector3 oldPos, newPos;
    void Start()
    {
        characterController.enabled = true;

        PlayerState.playerMovement = this;
        PlayerState.characterController = characterController;

        playerStates = new PlayerState[10];
        playerStates[0] = new PSIdle();
        playerStates[1] = new PSFalling();
        playerStates[2] = new PSWalking();
        playerStates[3] = new PSRunning();
        playerStates[4] = new PSJumping();

        currentPSE = PlayerStateE.idle;
        currentPS = playerStates[0];
    }

    void Update()
    {
        oldPos = newPos;
        newPos = characterController.transform.position;

        currentPS.Action();
        currentPSE = currentPS.Switch();
        currentPS = playerStates[(int)currentPSE];

        PlayerState.CalledEveryFrame();
        // Debug.Log(currentPS + ", " + currentPSE);

    }
    //-------------------------------------------------------------------------------------------------------------------------------//

    float movementSpeed = 0;
    const float maxMovementSpeed = 5;
    const float movementSpeedInc = 0.5f;
    void Move()
    {
        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;
        Vector3 playerForward = characterController.transform.forward;
        Vector3 playerSideway = characterController.transform.right;
        Vector3 newPlayerDir = (playerForward * zAxis) + (playerSideway * xAxis);
        newPlayerDir = newPlayerDir.normalized;

        characterController.Move(newPlayerDir * movementSpeed * Time.deltaTime);
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
    
    Vector3 cameraOldPos = Vector3.zero;
    Vector3 cameraNewPos = Vector3.zero;
    bool IsCameraMoving()
    {
        return cameraNewPos != cameraOldPos;
    }
    bool IsMoving()
    {
        if (playerInput.xAxis != 0 || playerInput.zAxis != 0)
            return true;
        return false;
    }
    bool IsFallingSoft()
    {
        bool b = !characterController.isGrounded;
        // Debug.Log("IsFalling: " + b);
        return b;
    }

    float hardFallDist = 0.05f;
    bool IsFallingHard()
    {
        return (oldPos.y - newPos.y) > hardFallDist;
    }

    float gravtiyVelo = 0;
    const float gravityAcc = 10f;
    const float terminalVelo = 40f;
    void ApplyGravity()
    {
        // Debug.Log(
        //     "Velo: " + gravtiyVelo + ", " +
        //     "TimeAdj: " + (-gravtiyVelo * Time.deltaTime)
        // );

        characterController.Move(new Vector3(0, -gravtiyVelo, 0) * Time.deltaTime);
    }

    //--------------------------------------------------------------------------------------------------------------------------//
    private enum PlayerStateE
    {
        idle, falling, walking, running, jumping
    }
    private abstract class PlayerState
    {
        public static PlayerMovement playerMovement;
        public static CharacterController characterController;
        public abstract void Action();
        public abstract PlayerStateE Switch();

        public static void CalledEveryFrame()
        {
            playerMovement.cameraOldPos = playerMovement.cameraNewPos;
            playerMovement.cameraNewPos = playerMovement.cameraMain.transform.position;
            if(playerMovement.IsCameraMoving())
                playerMovement.RotateFromCamera();

            if (playerMovement.IsMoving())
                playerMovement.movementSpeed = playerMovement.playerInput.SmoothValue(playerMovement.movementSpeed, 0, maxMovementSpeed, movementSpeedInc);
            else
                playerMovement.movementSpeed = playerMovement.playerInput.SmoothValue(playerMovement.movementSpeed, 0, maxMovementSpeed, -movementSpeedInc);

            if (playerMovement.IsFallingSoft())
                playerMovement.gravtiyVelo = playerMovement.playerInput.SmoothValue(
                    playerMovement.gravtiyVelo, 0, PlayerMovement.terminalVelo, PlayerMovement.gravityAcc);
            else
                playerMovement.gravtiyVelo = 0;

        }

    }
    private class PSIdle : PlayerState
    {
        public override void Action()
        {
            if (playerMovement.IsFallingSoft())
                playerMovement.ApplyGravity();
            // do nothing in idleV
        }
        public override PlayerStateE Switch()
        {

            if (playerMovement.IsFallingHard())
                return PlayerStateE.falling;
            if (playerMovement.IsMoving())
                return PlayerStateE.walking;
            return PlayerStateE.idle;
        }
    }
    private class PSFalling : PlayerState
    {
        public override void Action()
        {
            playerMovement.ApplyGravity();
        }
        public override PlayerStateE Switch()
        {
            if (playerMovement.IsFallingHard())
                return PlayerStateE.falling;
            if (playerMovement.IsMoving())
                return PlayerStateE.walking;
            return PlayerStateE.idle;
        }
    }
    private class PSWalking : PlayerState
    {
        public override void Action()
        {
            playerMovement.Move();
            if (playerMovement.IsFallingSoft())
                playerMovement.ApplyGravity();
        }
        public override PlayerStateE Switch()
        {
            if (playerMovement.IsFallingHard())
                return PlayerStateE.falling;
            if (playerMovement.IsMoving())
                return PlayerStateE.walking;
            return PlayerStateE.idle;
        }
    }
    private class PSRunning : PlayerState
    {
        public override void Action()
        {

        }
        public override PlayerStateE Switch()
        {
            return PlayerStateE.running;
        }
    }
    private class PSJumping : PlayerState
    {
        public override void Action()
        {

        }
        public override PlayerStateE Switch()
        {
            return PlayerStateE.jumping;
        }
    }
}