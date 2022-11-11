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

    void FixedUpdate()
    {
        UpdateVariables();
    }

    //-------------------------------------------------------------------------------------------------------------------------------//

    void UpdateVariables()
    {
        oldPos = newPos;
        newPos = characterController.transform.position;
    }

    //-------------------------------------------------------------------------------------------------------------------------------//

    Vector3 previousDirOfMotion;
    void Move(float magnitude)
    {

        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;

        Vector3 cameraForward = cameraMain.transform.forward;
        Vector3 cameraRight = cameraMain.transform.right;
        Vector3 playerForward = (new Vector3(cameraForward.x, 0, cameraForward.z)).normalized;
        Vector3 playerSideway = (new Vector3(cameraRight.x, 0, cameraRight.z)).normalized;
 
        Vector3 newPlayerDir;
        if (xAxis == 0 && zAxis == 0)
            newPlayerDir = previousDirOfMotion;
        else
            newPlayerDir = (playerForward * zAxis) + (playerSideway * xAxis);
        newPlayerDir = newPlayerDir.normalized;

        previousDirOfMotion = newPlayerDir;

        RotateInDirOfMotion(newPlayerDir);
        characterController.Move(newPlayerDir * magnitude);
    }

    const float jumpSpeed = 20f;
    void Jump(Vector3 playerDirWhenfirstJumped, float t)
    {
        Vector3 jumpDir = new Vector3(playerDirWhenfirstJumped.x, 0.9f, playerDirWhenfirstJumped.z);
        jumpDir = jumpDir.normalized;

        characterController.Move(jumpDir * jumpSpeed * Time.deltaTime * GiveCurrentJumpPower(t));

        float GiveCurrentJumpPower(float t)
        {
            return 4 * (-(t * t) + t);
        }
    }

    //--------------------------------------------------------------------------------------------------------------------------//

    float rotSpeed = 20;
    void RotateInDirOfMotion(Vector3 dirOfMotion)
    {
        Transform playerT = characterController.transform;
        Quaternion newRot = new Quaternion();
        newRot.SetLookRotation(dirOfMotion);

        playerT.rotation = Quaternion.Slerp(playerT.rotation, newRot, rotSpeed * Time.deltaTime);
    }
    //--------------------------------------------------------------------------------------------------------------------------//

    bool IsTryingMoving()
    {
        if (playerInput.xAxis != 0 || playerInput.zAxis != 0)
            return true;
        return false;
    }
    bool IsTryingJumping()
    {
        return playerInput.jump;
    }
    bool IsTryingRunning()
    {
         return IsTryingMoving() && playerInput.running;
    }


    //--------------------------------------------------------------------------------------------------------------------------//
    bool GroundCheck(float fallDist)
    {
        float quaterHeight = characterController.height / 4;
        Vector3 point = characterController.transform.TransformPoint(new Vector3(0, quaterHeight, 0));
        point += Vector3.up * 0.1f;

        RaycastHit raycastHit;
        return !Physics.SphereCast(point, characterController.radius, Vector3.down, out raycastHit, 0.1f + fallDist, groundLayer);
    }

    bool IsFallingSoft()
    {
        return GroundCheck(0.1f);
    }

    Vector3 oldPos, newPos;
    bool IsFallingHard()
    {
        // return (newPos.y - oldPos.y) > 0.2f;
        return GroundCheck(1f);
    }
    //--------------------------------------------------------------------------------------------------------------------------//

    TimeVariant gravity = new TimeVariant(0, 40, 2);
    void ApplyGravity()
    {
        gravity.Increment(1);
        gravity.Update();
        characterController.Move(new Vector3(0, -gravity.currentValue, 0) * Time.deltaTime);
    }
    void ResetGravity()
    {
        gravity.Reset();
    }
    void CheckAndApplyGravity()
    {
        if (IsFallingSoft())
            ApplyGravity();
        else
            ResetGravity();
    }
    //--------------------------------------------------------------------------------------------------------------------------//
    float Remap(float source, float sourceFrom, float sourceTo, float targetFrom, float targetTo)
    {
        return targetFrom + (source - sourceFrom) * (targetTo - targetFrom) / (sourceTo - sourceFrom);
    }
    //--------------------------------------------------------------------------------------------------------------------------//
    private enum PlayerStateE
    {
        idle, walking, running,falling, fallRecovery, jumping
    }
    private abstract class PlayerState
    {
        public static PlayerMovement playerMovement { get; private set; }
        public static CharacterController characterController { get; private set; }
        private static PlayerState[] playerStates;
        public static PlayerState currentPlayerState { get; private set; }

        public static void Init(PlayerMovement pm, CharacterController cc)
        {
            playerMovement = pm;
            characterController = cc;

            playerStates = new PlayerState[10];
            playerStates[0] = new PSIdle();
            playerStates[1] = new PSWalking();
            playerStates[2] = new PSRunning();
            playerStates[3] = new PSFalling();
            playerStates[4] = new PSFallRecovery();
            playerStates[5] = new PSJumping();


            currentPlayerState = playerStates[0];
        }
        public static void Update()
        {
            currentPlayerState.Action();
            currentPlayerState.CheckForSwitch();

            // Debug.Log("state: " + currentPlayerState);

            if (playerMovement.IsFallingSoft())
                playerMovement.ApplyGravity();
            else
                playerMovement.ResetGravity();
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
            if (playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);
            if (playerMovement.IsTryingJumping())
                SwitchState(PlayerStateE.jumping);
            if (playerMovement.IsTryingMoving())
                SwitchState(PlayerStateE.walking);

        }
    }

    private class PSWalking : PlayerState
    {
        bool isMoving = false;
        bool skippingReset = false;
        TimeVariant speed;
        public PSWalking()
        {
            speed = new TimeVariant(0, 10, 2);
        }
        public override void Action()
        {
            if (playerMovement.IsTryingMoving())
                speed.Increment(1);
            else
                speed.Decrement(5);

            speed.Update();

            if (speed.multipleFactorNormalized == 0)
            {
                isMoving = false;
                return;
            }
            playerMovement.Move(speed.currentValue * Time.deltaTime);
        }
        public override void Initialize()
        {
            isMoving = true;
        }
        public override void Terminate()
        {
            if (!skippingReset)
                speed.Reset();
            skippingReset = false;
        }
        public override void CheckForSwitch()
        {
            if (playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);
            if (playerMovement.IsTryingJumping())
            {
                skippingReset = true;
                SwitchState(PlayerStateE.jumping);
            }
            if(playerMovement.IsTryingRunning())
            {
                skippingReset= true;
                SwitchState(PlayerStateE.running);
            }
            if (!isMoving)
                SwitchState(PlayerStateE.idle);
        }
    }

    private class PSRunning : PlayerState
    {
        bool isRunning = false;
        bool skippingReset = false;
        TimeVariant speed;
        public PSRunning()
        {
            speed = new TimeVariant(10, 20, 2);
        }
        public override void Action()
        {
            if (playerMovement.IsTryingRunning())
                speed.Increment(1);
            else
                speed.Decrement(5);

            speed.Update();

            if (speed.multipleFactorNormalized == 0)
            {
                isRunning = false;
                return;
            }
            playerMovement.Move(speed.currentValue * Time.deltaTime);
        }
        public override void Initialize()
        {
            isRunning = true;
        }
        public override void Terminate()
        {
            if (!skippingReset)
                speed.Reset();
            skippingReset = false;
        }
        public override void CheckForSwitch()
        {
            if(playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);

            if(playerMovement.IsTryingJumping())
            {
                skippingReset = true;
                SwitchState(PlayerStateE.jumping);
            }

            if(!isRunning)
                SwitchState(PlayerStateE.walking);
        }
    }

    private class PSFalling : PlayerState
    {
        const float timeSinceFallingToGoToRecovery = 1f;
        float timeSinceStart = 0;
        bool isGoingToRevocery = false;

        public override void Action()
        {
            timeSinceStart += Time.deltaTime;
            if (timeSinceStart > timeSinceFallingToGoToRecovery)
                isGoingToRevocery = true;
        }
        public override void Initialize()
        { }
        public override void Terminate()
        {
            isGoingToRevocery = false;
            timeSinceStart = 0;
        }
        public override void CheckForSwitch()
        {
            if (!playerMovement.IsFallingHard())
            {
                if (isGoingToRevocery)
                    SwitchState(PlayerStateE.fallRecovery);
                else
                    SwitchState(PlayerStateE.idle);
            }
        }
    }
    private class PSFallRecovery : PlayerState
    {
        const float timeToRecovery = 2; //sec
        float timeSinceStart = 0;
        bool isRecovered = false;
        public override void Action()
        {
            timeSinceStart += Time.deltaTime;
            if (timeSinceStart > timeToRecovery)
                isRecovered = true;
        }
        public override void Initialize()
        { }
        public override void Terminate()
        {
            timeSinceStart = 0;
            isRecovered = false;
        }
        public override void CheckForSwitch()
        {
            if (playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);
            if (isRecovered)
                SwitchState(PlayerStateE.idle);
        }
    }
    private class PSJumping : PlayerState
    {
        TimeVariant jump;
        bool isJumpOver = false;
        Vector3 playerDirWhenJumpStart;

        public PSJumping()
        {
            jump = new TimeVariant(0, 1, 1);
        }
        public override void Action()
        {
            jump.Increment(1);
            jump.Update();

            playerMovement.Jump(playerDirWhenJumpStart, jump.currentValue);

            if (jump.currentValue == 1)
                isJumpOver = true;
        }
        public override void Initialize()
        {
            playerDirWhenJumpStart = characterController.transform.forward;
        }
        public override void Terminate()
        {
            jump.Reset();
            isJumpOver = false;
        }
        public override void CheckForSwitch()
        {
            if (isJumpOver)
                SwitchState(PlayerStateE.idle);
        }
    }
}
/*
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
*/