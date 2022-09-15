using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerController : MonoBehaviour
{

    [SerializeField] private CapsuleCollider playerCollider;
    [SerializeField] private SkinnedMeshRenderer myRenderer;
    [SerializeField] private GameObject player;
    [SerializeField] private PlayerInput playerInput;
    [SerializeField] private LayerMask groundLayer;
    [SerializeField] private Material markerMaterial;
    //------------------------------------------------------//
    PlayerStateE currentPlayerStateE;
    PlayerStateE[] currentPlayerStateEs;
    PlayerState currentPlayerState;
    PlayerState[] playerStates;
    //----------------------------------------------------//
    Vector3 colliderHalfExtents = new Vector3(1f / 2, 2f / 2, 1f / 2);
    Vector3 gravity;
    //--------------------------------------------------//

    void Start()
    {
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
    }

    void OnDrawGizmos()
    {
        float speed = 0.05f;
        float hori = Input.GetAxisRaw("Horizontal");
        float velo = Input.GetAxisRaw("Vertical");
        Vector3 oldPos = player.transform.position;
        Vector3 newPos = new Vector3(oldPos.x + (hori * speed), oldPos.y, oldPos.z + (velo * speed));
        Vector3 origin = new Vector3(newPos.x, newPos.y + 2, newPos.z);

        RaycastHit raycastHitInfo;
        if (Physics.Raycast(origin, Vector3.down, out raycastHitInfo, 5f, groundLayer))
        {
            Gizmos.color = Color.white;
            Gizmos.DrawLine(origin, raycastHitInfo.point);
            Vector3 normal = raycastHitInfo.normal;
            Vector3 reflected = Vector3.Reflect(Vector3.down, normal);

            RaycastHit raycastHitInfo2;
            Physics.Raycast(raycastHitInfo.point, reflected, out raycastHitInfo2, 5, groundLayer);
            Vector3 otherPoint = raycastHitInfo.point + reflected;

            Gizmos.color = Color.black;
            Gizmos.DrawLine(raycastHitInfo.point, otherPoint);
        }
    }


    void Update()
    {
        currentPlayerStateE = currentPlayerState.Update();
        currentPlayerState = playerStates[(int)currentPlayerStateE];
        currentPlayerState.Action();

        MoveA(player.transform.forward);

    }
    bool GroundCheck()
    {
        float quaterHeight = playerCollider.height * 0.25f;
        Vector3 point1 = playerCollider.transform.TransformPoint(new Vector3(0, quaterHeight, 0));
        Vector3 point2 = playerCollider.transform.TransformPoint(new Vector3(0, -quaterHeight, 0));

        return Physics.CheckCapsule(point1, point2, playerCollider.radius, groundLayer);
    }
    /*
    void Move(vector3 dir) {
        dir = dir.normalize;
        if(notGrounded)
            falling;
        
        check if move possible
            cast forward.
            if hit
                then check if we can climb over
                cast up from new pos
                if hit 
                    cant move forward
                    move side ways using normals
                else 
                    move forword to newPos
            else
                move forword regularly   
    }
    */
    GameObject marker, marker2, marker3, marker4, marker5;
    void MoveA(Vector3 dir)
    {
        float stepHeight = 1f;
        GameObject.Destroy(marker);
        GameObject.Destroy(marker2);
        GameObject.Destroy(marker3);
        float speed = 1f;
        Vector3 oldPos = player.transform.position + (Vector3.up * 0.01f);
        Vector3 newPos = oldPos + (dir * speed);

        RaycastHit raycastHitWallCheck;
        if (CheckCapsuleCast(playerCollider, playerCollider.transform.position, out raycastHitWallCheck, dir, speed))
        {
            // Debug.Log("HitWall!");
            marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker.transform.position = raycastHitWallCheck.point;
            marker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            //ray up to surface of wall
            RaycastHit raycastHitUpToWallSurface;
            Vector3 wallSurfaceorigin = newPos + (Vector3.up * stepHeight);

            // marker3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // marker3.transform.position = wallSurfaceorigin;
            // marker3.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

            if (Physics.Raycast(wallSurfaceorigin, Vector3.down, out raycastHitUpToWallSurface, stepHeight, groundLayer))
            {
                // Debug.Log("Hit Surface");
                marker2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker2.transform.position = raycastHitUpToWallSurface.point;
                marker2.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                // check if space above
                if (CheckCapsule(playerCollider, raycastHitUpToWallSurface.point + (Vector3.up * 0.01f)))
                {
                    Debug.Log("No space above");
                }

            }

        }
    }

    bool CheckCapsuleCast(CapsuleCollider collider, Vector3 position, out RaycastHit raycastHit, Vector3 dir, float maxDist)
    {
        Vector3 posOffset = collider.transform.position - position;
        float quaterHeight = collider.height * 0.25f;
        float radius = collider.radius;
        Vector3 point1 = collider.transform.TransformPoint(new Vector3(0, quaterHeight, 0)) + posOffset;
        Vector3 point2 = collider.transform.TransformPoint(new Vector3(0, collider.height - quaterHeight, 0)) + posOffset;


        return Physics.CapsuleCast(point1, point2, radius, dir, out raycastHit, radius + maxDist, groundLayer);

    }
    bool CheckCapsule(CapsuleCollider collider, Vector3 position)
    {
        Destroy(marker4);
        Destroy(marker5);
        Vector3 posOffset = position - collider.transform.position;
        float quaterHeight = collider.height * 0.25f;
        Vector3 point1 = collider.transform.TransformPoint(new Vector3(0, quaterHeight, 0)) + posOffset;
        Vector3 point2 = collider.transform.TransformPoint(new Vector3(0, collider.height - quaterHeight, 0)) + posOffset;

        marker4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker4.transform.position = point1;
        marker4.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        marker5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker5.transform.position = point2;
        marker5.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        
        return Physics.CheckCapsule(point1, point2, collider.radius, groundLayer);
    }



    void ApplyGravity()
    {
        player.transform.position += new Vector3(0, -0.02f, 0);
    }

    //-----------------------------------------------//

    private abstract class PlayerState
    {
        public static CapsuleCollider playerCollider;
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