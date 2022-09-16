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
    Vector3 gravityAcc = new Vector3(0, -0.02f, 0), gravityVelo;
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

        gravityVelo = new Vector3(0, 0, 0);
    }

    // void OnDrawGizmos()
    // {
    //     float speed = 0.05f;
    //     float hori = Input.GetAxisRaw("Horizontal");
    //     float velo = Input.GetAxisRaw("Vertical");
    //     Vector3 oldPos = player.transform.position;
    //     Vector3 newPos = new Vector3(oldPos.x + (hori * speed), oldPos.y, oldPos.z + (velo * speed));
    //     Vector3 origin = new Vector3(newPos.x, newPos.y + 2, newPos.z);

    //     RaycastHit raycastHitInfo;
    //     if (Physics.Raycast(origin, Vector3.down, out raycastHitInfo, 5f, groundLayer))
    //     {
    //         Gizmos.color = Color.white;
    //         Gizmos.DrawLine(origin, raycastHitInfo.point);
    //         Vector3 normal = raycastHitInfo.normal;
    //         Vector3 reflected = Vector3.Reflect(Vector3.down, normal);

    //         RaycastHit raycastHitInfo2;
    //         Physics.Raycast(raycastHitInfo.point, reflected, out raycastHitInfo2, 5, groundLayer);
    //         Vector3 otherPoint = raycastHitInfo.point + reflected;

    //         Gizmos.color = Color.black;
    //         Gizmos.DrawLine(raycastHitInfo.point, otherPoint);
    //     }
    // }


    void Update()
    {
        currentPlayerStateE = currentPlayerState.Update();
        currentPlayerState = playerStates[(int)currentPlayerStateE];
        currentPlayerState.Action();

        if (GroundCheck())
        {
            // Debug.Log("Grounded");
            MoveA(player.transform.forward);
            gravityVelo = Vector3.zero;
        }
        else
        {
            // Debug.Log("Gravity");
            ApplyGravity();
        }

        MoveA(player.transform.forward);

    }
    bool GroundCheck()
    {
        return CheckCapsule(playerCollider, player.transform.position - (Vector3.up * 0.01f));
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
    GameObject marker, marker2, marker3, marker4, marker5, marker6, marker7;
    void MoveA(Vector3 dir)
    {
        float radius = playerCollider.radius;
        float speed = 0.03f;
        float hori = Input.GetAxisRaw("Horizontal");
        float Vert = Input.GetAxisRaw("Vertical");

        if (hori == 0 && Vert == 0)
            return;

        dir = new Vector3(hori, 0, Vert).normalized;


        float stepHeight = 1.1f;
        GameObject.Destroy(marker);
        GameObject.Destroy(marker2);
        GameObject.Destroy(marker3);
        GameObject.Destroy(marker6);
        GameObject.Destroy(marker7);
        Vector3 oldPos = player.transform.position; // + (Vector3.up * 0.01f); // offset
        Vector3 newPos = oldPos + (dir * speed);

        RaycastHit raycastHitWallCheck;
        if (CheckCapsuleCast(playerCollider, oldPos, out raycastHitWallCheck, dir, speed))
        {
            Debug.Log("HitWall!");
            // marker = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            // marker.transform.position = raycastHitWallCheck.point;
            // marker.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            // marker.GetComponent<Renderer>().material.SetColor("_Color", Color.black);


            //ray up to surface of wall
            RaycastHit raycastHitUpToWallSurface;
            Vector3 wallSurfaceorigin = newPos + (Vector3.up * stepHeight);

            marker3 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker3.transform.position = wallSurfaceorigin;
            marker3.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            marker3.GetComponent<Renderer>().material.SetColor("_Color", Color.red);
            marker7 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
            marker7.transform.position = wallSurfaceorigin + (Vector3.down * stepHeight);
            marker7.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
            marker7.GetComponent<Renderer>().material.SetColor("_Color", Color.red);


            if (CheckCapsuleCast(playerCollider, wallSurfaceorigin, out raycastHitUpToWallSurface, Vector3.down, stepHeight))
            {
                Debug.Log("Hit Surface");
                marker2 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                marker2.transform.position = raycastHitUpToWallSurface.point;
                marker2.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);

                if (raycastHitUpToWallSurface.point.y - oldPos.y < stepHeight)
                {
                    Debug.Log("Value less than");
                    // check if space above
                    if (!CheckCapsule(playerCollider, raycastHitUpToWallSurface.point + (Vector3.up * 0.05f)))
                    {
                        Debug.Log("space above");
                        marker6 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
                        marker6.transform.position = raycastHitUpToWallSurface.point;
                        marker6.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
                        marker6.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);



                        player.transform.position = raycastHitUpToWallSurface.point+ (Vector3.up * 0.05f);
                    }
                }
            }

        }
        else
        {
            player.transform.position = newPos;
        }
    }

    bool CheckCapsuleCast(CapsuleCollider collider, Vector3 position, out RaycastHit raycastHit, Vector3 dir, float maxDist)
    {
        Destroy(marker4);
        Destroy(marker5);
        Vector3 posOffset = position - collider.transform.position;
        float quaterHeight = collider.height * 0.25f;
        float radius = collider.radius;
        Vector3 point1 = collider.transform.TransformPoint(new Vector3(0, quaterHeight, 0)) + posOffset;
        Vector3 point2 = collider.transform.TransformPoint(new Vector3(0, collider.height - quaterHeight, 0)) + posOffset;

        marker4 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker4.transform.position = point1;
        marker4.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        marker4.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);
        marker5 = GameObject.CreatePrimitive(PrimitiveType.Sphere);
        marker5.transform.position = point2;
        marker5.transform.localScale = new Vector3(0.2f, 0.2f, 0.2f);
        marker5.GetComponent<Renderer>().material.SetColor("_Color", Color.cyan);

        return Physics.CapsuleCast(point1, point2, radius, dir, out raycastHit, maxDist, groundLayer);

    }
    bool CheckCapsule(CapsuleCollider collider, Vector3 position)
    {
        Vector3 posOffset = position - collider.transform.position;
        float quaterHeight = collider.height * 0.25f;
        Vector3 point1 = collider.transform.TransformPoint(new Vector3(0, quaterHeight, 0)) + posOffset;
        Vector3 point2 = collider.transform.TransformPoint(new Vector3(0, collider.height - quaterHeight, 0)) + posOffset;


        return Physics.CheckCapsule(point1, point2, collider.radius, groundLayer);
    }



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