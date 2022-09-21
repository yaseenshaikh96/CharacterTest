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
    Vector3 playerDir, prePlayerDir;
    float hori, vert, minorOffset;
    float currentSpeed, speedInc, maxSpeed;
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
        minorOffset = 0.01f;
        currentSpeed = 0.03f;
        speedInc = 0.005f;
        maxSpeed = 0.07f;

        markerList.MakeMarkerVector("test vector up", Vector3.zero, Vector3.up);
        markerList.MakeMarkerVector("test vector down", Vector3.zero, Vector3.down);
        markerList.MakeMarkerVector("test vector xz", Vector3.zero, new Vector3(1, 0, 1).normalized);
        markerList.MakeMarkerVector("test vector yz", Vector3.zero, new Vector3(0, 1, 1).normalized);

        markerList.MakeMarkerVector("wallNormal", Vector3.zero, new Vector3(0, 1, 1).normalized);
        markerList.MakeMarkerVector("c1", Vector3.zero, new Vector3(0, 1, 1).normalized);
        markerList.MakeMarkerVector("c2", Vector3.zero, new Vector3(0, 1, 1).normalized);
        markerList.MakeMarkerVector("playerDir", Vector3.zero, new Vector3(0, 1, 1).normalized);

        markerList.MakeMarker("test", Vector3.one * 20);
        markerList.MakeMarker("FirstHit", Vector3.zero, Color.black);
        markerList.MakeMarker("oldPos", Vector3.zero, Color.white);
        markerList.MakeMarker("newPos", Vector3.zero, Color.black);
        markerList.MakeMarker("center", Vector3.zero, Color.black);
        markerList.MakeMarker("halfExtents", Vector3.zero, Color.black);
        markerList.MakeMarker("center2", Vector3.zero, Color.black);
        markerList.MakeMarker("halfExtents2", Vector3.zero, Color.black);
        markerList.MakeMarker("raycastOrigin", Vector3.zero, Color.red);
        markerList.MakeMarker("newPosYAdj", Vector3.zero, Color.magenta);
        markerList.MakeMarker("wallPoint", Vector3.zero, Color.magenta);
    }

    void Update()
    {

        hori = Input.GetAxisRaw("Horizontal");
        vert = Input.GetAxisRaw("Vertical");

        currentPlayerStateE = currentPlayerState.Update();
        currentPlayerState = playerStates[(int)currentPlayerStateE];
        currentPlayerState.Action();

        if (GroundCheck())
        {
            Debug.Log("Grounded");
            gravityVelo.y = 0;
            // Move2();
        }
        else
        {
            Debug.Log("Gravity");
            ApplyGravity();
        }


    }
    bool GroundCheck()
    {
        RaycastHit raycastHit;
        float groundCheckoffset = minorOffset * 10;
        if (CheckBoxCast(playerCollider, player.transform.position + (Vector3.up * groundCheckoffset), out raycastHit, Vector3.down, groundCheckoffset * 2))
        {
            Vector3 newPosYAdj = new Vector3(player.transform.position.x, raycastHit.point.y, player.transform.position.z);
            player.transform.position = newPosYAdj;
            return true;
        }
        else
        {
            return false;
        }
    }

    void Move2()
    {
        float stepHeight = 1f;
        Debug.Log("Speed: " + currentSpeed);

        if (hori == 0 && vert == 0)
        {
            playerDir = prePlayerDir;
            if (currentSpeed > 0)
                currentSpeed -= speedInc;
            else
                return;
        }
        else
        {
            playerDir = new Vector3(hori, 0, vert).normalized;
            if (currentSpeed < maxSpeed)
                currentSpeed += speedInc;
        }

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = oldPos + (playerDir * currentSpeed);

        markerList.UpdateMarker("oldPos", oldPos);
        markerList.UpdateMarker("newPos", newPos);


        RaycastHit raycastHit;
        if (CheckBoxCast(playerCollider, oldPos, out raycastHit, playerDir, currentSpeed))
        {
            // can step over?
            markerList.UpdateMarker("FirstHit", raycastHit.point);

            // check for ground over wall
            Vector3 raycastOrigin = newPos + (Vector3.up * stepHeight);
            markerList.UpdateMarker("raycastOrigin", raycastOrigin);
            RaycastHit raycastHit2;

            if (CheckBoxCastStep(playerCollider, raycastOrigin, out raycastHit2, Vector3.down, stepHeight))
            {
                Vector3 newPosYAdj = new Vector3(newPos.x, raycastHit2.point.y, newPos.z);

                if (!CheckBox(playerCollider, newPosYAdj + (Vector3.up * minorOffset)))
                {
                    // can climb over!
                    // Debug.Log("can climb over!");
                    markerList.UpdateMarker("newPosYAdj", raycastOrigin);
                    player.transform.position = newPosYAdj;
                }
                else
                {
                    // Debug.Log("cannot climb over");
                    // cannot climb over :(


                    PushSideways(raycastHit);
                    // push sideways
                }
            }
            else
            {
                PushSideways(raycastHit);
                // Debug.Log("climbOver check no hit");
                // no space above :(
                // push sideways
            }
        }
        else
        {
            player.transform.position = newPos;
            Debug.Log("no Hit!");
        }
        prePlayerDir = playerDir;

        void PushSideways(RaycastHit wallHit)
        {
            Vector2 wallNormal = wallHit.normal;
            Vector3 wallPoint = wallHit.point;
            Vector3 c1 = Vector3.Cross(wallNormal, playerDir);
            Vector3 wallTangent = Vector3.Cross(c1, wallNormal);

            markerList.UpdateMarker("wallPoint", wallPoint);

            markerList.UpdateVector("wallNormal", wallPoint, wallNormal);
            markerList.UpdateVector("playerDir", wallPoint, playerDir);
            markerList.UpdateVector("c1", wallPoint, c1);
            markerList.UpdateVector("c2", wallPoint, wallTangent);

        }
    }

    bool CheckBoxCastStep(BoxCollider collider, Vector3 position, out RaycastHit raycastHit, Vector3 dir, float maxDist)
    {
        float y = 0.05f;
        Vector3 center = position + (Vector3.up * (y / 2));
        Vector3 halfExtents = new Vector3(collider.size.x, y / 2, collider.size.z);
        markerList.UpdateMarker("center2", center);
        markerList.UpdateMarker("halfextents2", halfExtents);

        return Physics.BoxCast(center, halfExtents, dir, out raycastHit, Quaternion.identity, maxDist, groundLayer);
    }
    bool CheckBoxCast(BoxCollider collider, Vector3 position, out RaycastHit raycastHit, Vector3 dir, float maxDist)
    {
        Vector3 center = position + (Vector3.up * (collider.size.y / 2));
        Vector3 halfExtents = collider.size / 2;
        markerList.UpdateMarker("center", center);
        markerList.UpdateMarker("halfextents", halfExtents);

        return Physics.BoxCast(center, halfExtents, dir, out raycastHit, Quaternion.identity, maxDist, groundLayer);
    }

    bool CheckBox(BoxCollider collider, Vector3 position)
    {
        Vector3 center = position + (Vector3.up * (collider.size.y / 2));
        Vector3 halfExtents = collider.size / 2;
        return Physics.CheckBox(center, halfExtents, Quaternion.identity, groundLayer);
    }

    void ApplyGravity()
    {
        gravityVelo += gravityAcc;

        Vector3 oldPos = player.transform.position;
        Vector3 newPos = player.transform.position + gravityVelo;

        RaycastHit raycastHit;

        if (CheckBoxCast(playerCollider, oldPos, out raycastHit, Vector3.down, Mathf.Abs(gravityVelo.y)))
        {
            Vector3 newPosYAdj = new Vector3(newPos.x, raycastHit.point.y, newPos.z);
            player.transform.position = newPosYAdj;
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