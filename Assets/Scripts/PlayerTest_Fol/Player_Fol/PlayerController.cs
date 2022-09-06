using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private BoxCollider myCollider;
    [SerializeField] private SkinnedMeshRenderer myRenderer;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerInput playerInput;

    PlayerStateE currentPlayerStateE;
    PlayerStateE[] currentPlayerStateEs;
    PlayerState currentPlayerState;
    PlayerState[] playerStates;

    void Start()
    {
        PlayerState.playerController = this;
        PlayerState.playerInput = playerInput;
        currentPlayerStateE = PlayerStateE.idle;
        playerStates = new PlayerState[10];
        playerStates[0] = new PSIdle(); 
        playerStates[1] = new PSWalking(); 
        playerStates[2] = new PSRunning(); 
        playerStates[3] = new PSJumpStart(); 
        playerStates[4] = new PSJumping();
        currentPlayerState = playerStates[0];
    }


    void Update()
    {
        currentPlayerStateE = currentPlayerState.Update();
        currentPlayerState = playerStates[(int)currentPlayerStateE];
        currentPlayerState.Action();

        Debug.Log(
            "currentPlayerStateE: " + currentPlayerStateE.ToString() + "\n" +
            "currentPlayerState : " + currentPlayerState.ToString()
        );
    }

    //-----------------------------------------------//

    private abstract class PlayerState
    {
        public static PlayerController playerController;
        public static PlayerInput playerInput;
        public abstract PlayerStateE Update();
        public abstract void Action();
    }
    //-----------------------------------------------//
    private class PSIdle : PlayerState
    {
        
        public override PlayerStateE Update()
        {
            if(playerInput.xAxis != 0 || playerInput.yAxis != 0)
                return PlayerStateE.walking;
            return PlayerStateE.idle;
        }
        public override void Action()
        {
        }
    }
    //-----------------------------------------------//
    private class PSWalking : PlayerState
    {
        public override PlayerStateE Update()
        {

            if(playerInput.xAxis != 0 || playerInput.yAxis != 0)
                if(playerInput.running)
                    return PlayerStateE.running;
                else
                    return PlayerStateE.walking;
            
            return PlayerStateE.idle;
        }
        public override void Action()
        {
        }
    }
    //-----------------------------------------------//
    private class PSRunning : PlayerState
    {
        public override PlayerStateE Update()
        {
            if(playerInput.running)
                if(playerInput.xAxis != 0 || playerInput.yAxis != 0)
                    return PlayerStateE.running;
            return PlayerStateE.walking;
        }
        public override void Action()
        {
        }
    }
    //-----------------------------------------------//
    private class PSJumpStart : PlayerState
    {
        public override PlayerStateE Update()
        {
            return PlayerStateE.walking;
        }
        public override void Action()
        {
        }
    }
    //-----------------------------------------------//
    private class PSJumping : PlayerState
    {
        public override PlayerStateE Update()
        {
            return PlayerStateE.walking;
        }
        public override void Action()
        {
        }
    }

}
public enum PlayerStateE
{
    idle, walking, running,
    jumpStart, jumping,
    fallStart, falling,
    attack1Start, attack1ing,
    
}
