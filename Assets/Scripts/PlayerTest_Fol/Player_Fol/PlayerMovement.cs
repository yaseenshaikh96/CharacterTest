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

    void Jump()
    {
        
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
    float gravityVelo;
    const float terminalVelo = 40f;
    const float gravityAcc = terminalVelo / 120; //denominator frames for terminal
    void ApplyGravity()
    {
        gravityVelo += gravityAcc;
        gravityVelo = Mathf.Clamp(gravityVelo, 0, terminalVelo);
        characterController.Move(new Vector3(0, -gravityVelo, 0) * Time.deltaTime);
    }
    void ResetGravity()
    {
        gravityVelo = 0;
    }
    //--------------------------------------------------------------------------------------------------------------------------//
    private enum PlayerStateE
    {
        idle, walking, falling, fallRecovery, jumping
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
            playerStates[3] = new PSFallRecovery();
            playerStates[4] = new PSJumping();


            currentPlayerState = playerStates[0];
        }
        public static void Update()
        {
            currentPlayerState.Action();
            currentPlayerState.CheckForSwitch();

            Debug.Log("state: " + currentPlayerState);

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
            if (playerMovement.IsTryingMoving())
                SwitchState(PlayerStateE.walking);

        }
    }

    private class PSWalking : PlayerState
    {
        private const float maxSpeed = 10f; // 10 m/s
        bool isMoving = false;
        private float speedFactor = 0;
        private float speedFactorTimeAdj = 0;
        private const float timeTillMaxSpeed = 1; // sec

        public override void Action()
        {
            if (playerMovement.IsTryingMoving())
                speedFactor += Time.deltaTime;
            else
                speedFactor -= 5 * Time.deltaTime;

            speedFactor = Mathf.Clamp(speedFactor, 0, timeTillMaxSpeed);
            speedFactorTimeAdj = Remap(speedFactor, 0, timeTillMaxSpeed, 0, 1);

            if (speedFactorTimeAdj == 0)
            {
                isMoving = false;
                return;
            }
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
            if (playerMovement.IsFallingHard())
                SwitchState(PlayerStateE.falling);
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