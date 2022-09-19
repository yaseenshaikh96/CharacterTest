using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private BoxCollider playerCollider;
    [SerializeField] private SkinnedMeshRenderer myRenderer;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material markerMaterial;
    //------------------------------------------------------//
    PlayerStateE currentPlayerStateE;
    PlayerState currentPlayerState;
    PlayerState[] playerStates;
    //----------------------------------------------------//
    Vector3 gravityAcc = new Vector3(0, -0.02f, 0), gravityVelo;
    Vector3 playerDir;
    float hori, vert;
    MarkerList markerList;
    //--------------------------------------------------//

    void Start()
    {
        markerList = new MarkerList();
        PlayerState.playerController = this;
        PlayerState.playerInput = playerInput;
        PlayerState.playerCollider = playerCollider;
        PlayerState.groundLayer = groundLayer;

        currentPlayerStateE = PlayerStateE.idle;
        playerStates = new PlayerState[10];
        playerStates[0] = new PSIdle();
        playerStates[1] = new PSWalking();
        playerStates[2] = new PSRunning();
        playerStates[3] = new PSJumpStart();
        playerStates[4] = new PSJumping();
        playerStates[5] = new PSJumpEnd();
        playerStates[6] = new PSDash();
        currentPlayerState = playerStates[0];

        gravityVelo = new Vector3(0, 0, 0);

        markerList.MakeMarker("test", Vector3.one * 20);
        markerList.MakeMarker("FirstHit", Vector3.zero, Color.black);
        markerList.MakeMarker("oldPos", Vector3.zero, Color.white);
        markerList.MakeMarker("newPos", Vector3.zero, Color.black);
        markerList.MakeMarker("center", Vector3.zero, Color.black);
        markerList.MakeMarker("halfExtents", Vector3.zero, Color.black);

    }

    void Update()
    {

        hori = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");

        currentPlayerStateE = currentPlayerState.Update();
        currentPlayerState = playerStates[(int)currentPlayerStateE];
        currentPlayerState.Action();

        Move2();

    }
    bool GroundCheck()
    {
        return false;
        // return CheckCapsule(playerCollider, player.transform.position - (Vector3.up * 0.01f));
    }

    void Move2()
    {
        float speed = 0.03f;
        // float stepHeight = 1f;

        // if (hori == 0 && vert == 0)
        //     return;

        playerDir = new Vector3(hori, 0, vert).normalized;

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = oldPos + (playerDir * speed);

        markerList.UpdateMarker("oldPos", oldPos);
        markerList.UpdateMarker("newPos", newPos);


        RaycastHit raycastHit;
        if (CheckBoxCast(playerCollider, oldPos, out raycastHit, Vector3.down, 1f))
        {
            Debug.Log("Hit!");
        }else {

            Debug.Log("no Hit!");
        }

    }

    bool CheckBoxCast(BoxCollider collider, Vector3 position, out RaycastHit raycastHit, Vector3 dir, float maxDist)
    {
        Vector3 center = collider.transform.position + (Vector3.up * (collider.size.y / 2));
        Vector3 halfExtents = collider.size / 2;
        markerList.UpdateMarker("center", center);
        markerList.UpdateMarker("halfextents", halfExtents);

        Debug.Log("center: " + center + ", " + "halfExtents: " + halfExtents);

        return Physics.BoxCast(center, halfExtents, dir, out raycastHit, Quaternion.identity, maxDist, groundLayer);
    }
    // bool CheckCapsule(BoxCollider collider, Vector3 position)
    // {
    //     Vector3 posOffset = position - collider.transform.position;
    //     float quaterHeight = collider.height * 0.25f;
    //     Vector3 point1 = collider.transform.TransformPoint(new Vector3(0, quaterHeight, 0)) + posOffset;
    //     Vector3 point2 = collider.transform.TransformPoint(new Vector3(0, collider.height - quaterHeight, 0)) + posOffset;
    //     return Physics.CheckCapsule(point1, point2, collider.radius, groundLayer);
    // }

    void ApplyGravity()
    {
        gravityVelo += gravityAcc;

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = player.transform.position + gravityVelo;

        RaycastHit raycastHit;
        if (Physics.Raycast(oldPos, Vector3.down, out raycastHit, Mathf.Abs(gravityVelo.y), groundLayer))
        {
            player.transform.position = raycastHit.point;
        }
        else
        {

            player.transform.position = newPos;
        }


    }

    //-----------------------------------------------//

    private abstract class PlayerState
    {
        public static BoxCollider playerCollider;
        public static PlayerController playerController;
        public static PlayerInput playerInput;
        public static LayerMask groundLayer;
        public abstract PlayerStateE Update();
        public abstract void Action();
    }
    //-----------------------------------------------//
    private class PSIdle : PlayerState
    {

        public override PlayerStateE Update()
        {
            // if(playerInput.xAxis != 0 || playerInput.yAxis != 0)
            //     return PlayerStateE.walking;




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

            if (playerInput.xAxis != 0 || playerInput.yAxis != 0)
                if (playerInput.running)
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
            if (playerInput.running)
                if (playerInput.xAxis != 0 || playerInput.yAxis != 0)
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
    //-----------------------------------------------//
    private class PSJumpEnd : PlayerState
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
    private class PSDash : PlayerState
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
    jumpStart, jumping, jumpEnd,
    dash,
    fallStart, falling,
    attack1Start, attack1ing,

}
// void OnDrawGizmos()
// {
//     float radius = playerCollider.radius;
//     float speed = 0.03f;


//     playerDir = player.transform.forward;
//     Vector3 oldPos = player.transform.position;
//     Vector3 newPos = oldPos + (playerDir * speed);

//     RaycastHit raycastHit;
//     if (Physics.Raycast(oldPos, Vector3.down, out raycastHit, 0.5f, groundLayer))
//     {
//         Vector3 surfacePoint = raycastHit.point;
//         Vector3 surfaceNormal = raycastHit.normal;

//         DrawVector(surfacePoint, surfaceNormal, Color.black);
//         DrawVector(surfacePoint, playerDir, Color.magenta);

//         Vector3 c1 = Vector3.Cross(surfaceNormal, playerDir);
//         DrawVector(surfacePoint, c1, Color.red);

//         Vector3 c2 = Vector3.Cross(c1, surfaceNormal);
//         DrawVector(surfacePoint, c2, Color.gray);
//     }

//     void DrawVector(Vector3 position, Vector3 dir, Color color)
//     {
//         Gizmos.color = color;
//         Gizmos.DrawLine(position, position + dir);
//     }
// }