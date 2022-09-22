using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerMovement : MonoBehaviour
{
    [SerializeField] private CharacterController characterController;
    [SerializeField] private PlayerInput playerInput;

    private PlayerState[] playerStates;
    private PlayerState currentPS;
    private PlayerStateE currentPSE;
    //-----------------------------------------//
    private Vector3 oldPos, newPos;

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

        Debug.Log(currentPS + ", " + currentPSE);

    }
    //-------------------------------------------------------------------------------------------------------------------------------//

    void Move()
    {
        float speed = 0.1f;
        float xAxis = playerInput.xAxis;
        float zAxis = playerInput.zAxis;

        Vector3 playerDir = new Vector3(xAxis, 0, zAxis).normalized;


        characterController.Move(playerDir * speed);
    }

    //--------------------------------------------------------------------------------------------------------------------------//

    bool IsMoving()
    {
        if (playerInput.xAxis != 0 || playerInput.zAxis != 0)
            return true;
        return false;
    }
    bool IsFallingSoft()
    {
        return !characterController.isGrounded;
    }

    float hardFallDist = 0.05f;    
    bool IsFallingHard()
    {
        return (oldPos.y - newPos.y) > hardFallDist;
    }

    float gravtiyVelo = 0;
    const float gravityAcc = 0.3f;
    const float terminalVelo = 3f;
    void ApplyGravity()
    {
        gravtiyVelo += gravityAcc;
        gravtiyVelo = Mathf.Clamp(gravtiyVelo, 0, terminalVelo);

        characterController.Move(new Vector3(0, -gravtiyVelo, 0));
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
    }
    private class PSIdle : PlayerState
    {
        public override void Action()
        {
            if(playerMovement.IsFallingSoft())
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
            if(playerMovement.IsFallingSoft())
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
}