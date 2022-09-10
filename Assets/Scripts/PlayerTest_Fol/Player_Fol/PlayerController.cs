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
    //------------------------------------------------------//
    PlayerStateE currentPlayerStateE;
    PlayerStateE[] currentPlayerStateEs;
    PlayerState currentPlayerState;
    PlayerState[] playerStates;
    //----------------------------------------------------//
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
        playerStates[4] = new PSJumpEnd();
        playerStates[4] = new PSDash();
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

        if (GroundCheck())
        {
            Debug.Log("Grounded");
            Move();
        }
        else
        {
            ApplyGravity();
        }

    }
    bool GroundCheck()
    {
        Vector3 halfExtents = new Vector3(1f / 2, 2f / 2, 1f / 2);
        Vector3 center = player.transform.TransformPoint(new Vector3(0, 1, 0));
        return Physics.CheckBox(center, halfExtents, player.transform.rotation, groundLayer);
    }

    void Move()
    {
        float speed = 0.05f;
        float stepAngle = 45f; //deg
        float stepHeight = 0.05f;
        float hori = Input.GetAxisRaw("Horizontal");
        float velo = Input.GetAxisRaw("Vertical");

        Vector3 moveDir = (new Vector3(hori, 0, velo)).normalized;

        Vector3[] rayOrigins = new Vector3[5];
        rayOrigins[0] = player.transform.TransformPoint(new Vector3(0, 2, 0)); // center
        rayOrigins[0] = player.transform.TransformPoint(new Vector3(0.5f, 2, 0.5f)); // top right
        rayOrigins[0] = player.transform.TransformPoint(new Vector3(-0.5f, 2, 0.5f)); // top left
        rayOrigins[0] = player.transform.TransformPoint(new Vector3(0.5f, 2, -0.5f)); // bot right
        rayOrigins[0] = player.transform.TransformPoint(new Vector3(-0.5f, 2, -0.5f)); // bot left

        //slop

        for (int i = 1; i < 5; i++)
        {
            Vector3 oldRayOrigin = rayOrigins[i];
            Vector3 oldPos = player.transform.position;
            Vector3 newPos = new Vector3(oldPos.x + (hori * speed), oldPos.y, oldPos.z + (velo * speed));
            Vector3 newRayOrigin = new Vector3(newPos.x, newPos.y + 2, newPos.z);

            RaycastHit newOriginToDownHitInfo;
            RaycastHit OldToNewOriginHitInfo;
            if (!Physics.Raycast(oldRayOrigin, moveDir, out OldToNewOriginHitInfo, speed, groundLayer))
            {
                if (Physics.Raycast(newRayOrigin, Vector3.down, out newOriginToDownHitInfo, 2.1f, groundLayer))
                {

                    Vector3 groundPoint = newOriginToDownHitInfo.point;
                    Vector3 groundNormal = newOriginToDownHitInfo.normal;
                    Vector3 reflectedDir = Vector3.Reflect(Vector3.down, groundNormal);

                    float angle = Vector3.Angle(groundNormal, reflectedDir);

                    if (angle > stepAngle)
                    {
                        Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.z);
                        player.transform.position += delfectedDir * speed;
                    }
                    else
                    {
                        if ((groundPoint.y - newPos.y) < stepHeight)
                        {

                            Vector3 movePos = new Vector3(newPos.x, groundPoint.y, newPos.z);
                            player.transform.position = movePos;
                        }
                        else
                        {
                            Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.y);
                            player.transform.position += delfectedDir * speed * 4;
                        }
                    }
                }
                else
                {
                    player.transform.position = newPos;
                }
            }
            else
            {
                Vector3 wallNormal = OldToNewOriginHitInfo.normal;
                Vector3 reflectedDir = Vector3.Reflect(moveDir, wallNormal);

                Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.z);
                player.transform.position += delfectedDir * speed;

                break;
            }

        }

        //wall check
        bool wallHit = false;
        for (int i = 1; i < 5; i++)
        {
            Vector3 oldRayOrigin = rayOrigins[i];
            Vector3 oldPos = player.transform.position;
            Vector3 newPos = new Vector3(oldPos.x + (hori * speed), oldPos.y, oldPos.z + (velo * speed));
            Vector3 newRayOrigin = new Vector3(newPos.x, newPos.y + 2, newPos.z);

            RaycastHit OldToNewOriginHitInfo;
            if (Physics.Raycast(oldRayOrigin, moveDir, out OldToNewOriginHitInfo, speed, groundLayer))
            {
                wallHit = true;

                Vector3 wallNormal = OldToNewOriginHitInfo.normal;
                Vector3 reflectedDir = Vector3.Reflect(moveDir, wallNormal);

                Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.z);
                player.transform.position += delfectedDir * speed;

                break;
            }
        }

        if (wallHit) return;

        //slop

        {
            Vector3 oldRayOrigin = rayOrigins[0];
            Vector3 oldPos = player.transform.position;
            Vector3 newPos = new Vector3(oldPos.x + (hori * speed), oldPos.y, oldPos.z + (velo * speed));
            Vector3 newRayOrigin = new Vector3(newPos.x, newPos.y + 2, newPos.z);

            RaycastHit newOriginToDownHitInfo;
            if (Physics.Raycast(newRayOrigin, Vector3.down, out newOriginToDownHitInfo, 2.1f, groundLayer))
            {

                Vector3 groundPoint = newOriginToDownHitInfo.point;
                Vector3 groundNormal = newOriginToDownHitInfo.normal;
                Vector3 reflectedDir = Vector3.Reflect(Vector3.down, groundNormal);

                float angle = Vector3.Angle(groundNormal, reflectedDir);

                if (angle > stepAngle)
                {
                    Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.z);
                    player.transform.position += delfectedDir * speed;
                }
                else
                {
                    if ((groundPoint.y - newPos.y) < stepHeight)
                    {

                        Vector3 movePos = new Vector3(newPos.x, groundPoint.y, newPos.z);
                        player.transform.position = movePos;
                    }
                    else
                    {
                        Vector3 delfectedDir = new Vector3(reflectedDir.x, 0, reflectedDir.y);
                        player.transform.position += delfectedDir * speed * 4;
                    }
                }
            }
            else
            {
                player.transform.position = newPos;
            }
        }

    }



    void ApplyGravity()
    {
        player.transform.position += new Vector3(0, -0.02f, 0);
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
//
/*
        float upOffset = 0f;
        Vector3 center = player.transform.TransformPoint(new Vector3(0, 1, 0));

        while(Physics.CheckBox(center, halfExtents, player.transform.rotation, groundLayer)) {
            Debug.Log("time: " + Time.deltaTime);
            center = player.transform.TransformPoint(new Vector3(0, center.y + 0.01f, 0));
            upOffset += 0.02f;
        }
*/
/*
    bool GroundCheck()
    {
        Vector3 center = player.transform.TransformPoint(new Vector3(0, 1, 0));
        return Physics.CheckBox(center, halfExtents, player.transform.rotation, groundLayer);
    }
    void ApplyGravity() 
    {
        velocity.y =  -0.02f;
    }

    void ApplyVelocity()
    {
        player.transform.position += velocity;
        player.transform.position = new Vector3(player.transform.position.x, upOffset,player.transform.position.z);
    }

    void Move() 
    {
        float stepHeight = 0.1f;
        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 oldPos = player.transform.position;
        Vector3 newPos= new Vector3(oldPos.x + (horizontal * 0.08f), oldPos.y, oldPos.z + (vertical * 0.08f));

        RaycastHit raycastHitInfo;
        Physics.Raycast(newPos, Vector3.down, out raycastHitInfo, 2f, groundLayer);

        if(raycastHitInfo.point.y - newPos.y < stepHeight)    
        {
            Debug.Log("ray.y: " + raycastHitInfo.point.y + " newpos.y: " + newPos.y + " diff: " + (raycastHitInfo.point.y - newPos.y));
            velocity.z += vertical * 0.08f;
            velocity.x += horizontal * 0.08f;
            upOffset = raycastHitInfo.point.y;
        }else {
            upOffset = oldPos.y;
        }

    }
    bool GroundAngleCheck()
    {
        Vector3 center = groundAngleCollider.transform.TransformPoint(new Vector3(0, 0.1f, 0));
        Vector3 halfExtents = new Vector3(1.2f / 2, 0.2f / 2, 1.2f / 2);
        return Physics.CheckBox(center, halfExtents, player.transform.rotation, groundLayer);
    }
*/