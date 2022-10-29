using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AI : MonoBehaviour
{
    [SerializeField] CharacterController enemyController;
    [SerializeField] GameObject playerGO, EnemyGO;
    [SerializeField] LayerMask groundLayer;
    [SerializeField, Range(10, 200)] float unloadDistance;
    [SerializeField] float chasingDistance;
    [SerializeField] float attackDistance;
    //------------------------------------------------------------------//
    Vector3 playerWorldPos, enemyWorldPosNew, enemyWorldPosOld;
    Vector2 playerWorldPos2D, enemyWorldPos2D;
    Vector3 previousDirOfMotion;
    //-----------------------------------------------------------------//
    void Start()
    {
        playerWorldPos2D = new Vector2();
        enemyWorldPos2D = new Vector2();
        EnemyState.Init(this, enemyController);
    }
    void Update()
    {
        if (IsOutOfRange())
        {
            return;
        }
        UpdateVariables();
        EnemyState.Update();
    }
    void UpdateVariables()
    {
        playerWorldPos = playerGO.transform.position;
        playerWorldPos2D.x = playerWorldPos.x;
        playerWorldPos2D.y = playerWorldPos.z;

        enemyWorldPosOld = enemyWorldPosNew;
        
        enemyWorldPosNew = EnemyGO.transform.position;
        enemyWorldPos2D.x = enemyWorldPosNew.x;
        enemyWorldPos2D.y = enemyWorldPosNew.z;

    }
    //-----------------------------------------------------------------//
    bool IsOutOfRange()
    {
        return Vector2.Distance(playerWorldPos2D, enemyWorldPos2D) > unloadDistance;
    }
    void AIMove(float magnitude)
    {
        Vector3 moveDirection= playerWorldPos - enemyWorldPosNew;
        moveDirection.y = 0;
        moveDirection = moveDirection.normalized;
        Move(moveDirection, magnitude);

    }
    void Move(Vector3 newDir, float magnitude)
    {
        if (newDir == Vector3.zero)
            newDir = previousDirOfMotion;
        previousDirOfMotion = newDir;

        RotateInDirOfMotion(newDir);
        enemyController.Move(newDir * magnitude * Time.deltaTime);
    }
    float rotSpeed = 20;
    void RotateInDirOfMotion(Vector3 dirOfMotion)
    {
        Quaternion newRot = new Quaternion();
        newRot.SetLookRotation(dirOfMotion);
        Transform enemyT = enemyController.transform;
        enemyT.rotation = Quaternion.Slerp(enemyT.rotation, newRot, rotSpeed * Time.deltaTime);
    }
    //-------------------------------------------------------------------------//
    bool GroundCheck(float fallDist)
    {
        float quaterHeight = enemyController.height / 4;
        Vector3 point = enemyController.transform.TransformPoint(new Vector3(0, quaterHeight, 0));
        point += Vector3.up * 0.1f;

        RaycastHit raycastHit;
        return !Physics.SphereCast(point, enemyController.radius, Vector3.down, out raycastHit, 0.1f + fallDist, groundLayer);
    }
    //-------------------------------------------------------------------------//
    TimeVariant gravity = new TimeVariant(0, 40, 2);
    void ApplyGravity()
    {
        gravity.Increment(1);
        gravity.Update();
        enemyController.Move(new Vector3(0, -gravity.currentValue, 0) * Time.deltaTime);
    }
    void ResetGravity()
    {
        gravity.Reset();
    }
    bool IsFallingSoft()
    {
        return GroundCheck(0.1f);
    }
    bool IsFallingHard()
    {
        return GroundCheck(1f);
    }
    //-------------------------------------------------------------------------//
    bool IsInAttackRange()
    {
        return Vector3.Distance(playerWorldPos, enemyWorldPosNew) < attackDistance;
    }
    bool IsInChasingDistance()
    {
        return Vector3.Distance(playerWorldPos, enemyWorldPosNew) < chasingDistance;
    }

    //-------------------------------------------------------------------------//
    private enum EnemyStateE
    {
        idle, chasing, falling, fallRecovery, attacking
    }
    private abstract class EnemyState
    {
        public static Enemy_AI enemy_AI { get; private set; }
        public static CharacterController enemyController { get; private set; }
        private static EnemyState[] enemyStates;
        public static EnemyState currentEnemyState { get; private set; }

        public static void Init(Enemy_AI ai, CharacterController ec)
        {
            enemy_AI = ai;
            enemyController = ec;

            enemyStates = new EnemyState[10];
            enemyStates[0] = new ESIdle();
            enemyStates[1] = new ESChasing();
            enemyStates[2] = new ESFalling();
            enemyStates[3] = new ESFallRecovery();
            enemyStates[4] = new ESAttacking();

            currentEnemyState = enemyStates[0];
        }
        public static void Update()
        {
            currentEnemyState.Action();
            currentEnemyState.CheckForSwitch();

            Debug.Log("State: " + currentEnemyState);

            if (enemy_AI.IsFallingSoft())
                enemy_AI.ApplyGravity();
            else
                enemy_AI.ResetGravity();
        }
        protected void SwitchState(EnemyStateE newState)
        {
            currentEnemyState.Terminate();
            currentEnemyState = enemyStates[(int)newState];
            currentEnemyState.Initialize();
        }

        public abstract void Action();
        public abstract void CheckForSwitch();
        public abstract void Initialize();
        public abstract void Terminate();
    }
    class ESIdle : EnemyState
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
            if(enemy_AI.IsFallingHard())
                SwitchState(EnemyStateE.falling);
            if(enemy_AI.IsInChasingDistance())
                SwitchState(EnemyStateE.chasing);
        }
    }
    class ESChasing : EnemyState
    {
        bool isMoving = false;
        TimeVariant speed;
        public ESChasing()
        {
            speed = new TimeVariant(0, 10, 2);
        }
        public override void Action()
        {
            if (enemy_AI.IsInChasingDistance())
                speed.Increment(1);
            else
                speed.Decrement(5);

            speed.Update();

            if (speed.multipleFactorNormalized == 0)
            {
                isMoving = false;
                return;
            }
            enemy_AI.AIMove(speed.currentValue);
        }
        public override void Initialize()
        {
            isMoving = true;
        }
        public override void Terminate()
        {
            speed.Reset();
        }
        public override void CheckForSwitch()
        {
            if (enemy_AI.IsFallingHard())
                SwitchState(EnemyStateE.falling);
            if(enemy_AI.IsInAttackRange())
                SwitchState(EnemyStateE.attacking);
            if (!isMoving)
                SwitchState(EnemyStateE.idle);
        }
    }
    class ESFalling : EnemyState
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
            if (!enemy_AI.IsFallingHard())
            {
                if (isGoingToRevocery)
                    SwitchState(EnemyStateE.fallRecovery);
                else
                    SwitchState(EnemyStateE.idle);
            }
        }
    }
    class ESFallRecovery : EnemyState
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
            if (enemy_AI.IsFallingHard())
                SwitchState(EnemyStateE.falling);
            if (isRecovered)
                SwitchState(EnemyStateE.idle);
        }
    }
    class ESAttacking : EnemyState
    {
        public override void Action()
        {
            // attack!
        }
        public override void Initialize()
        { }
        public override void Terminate()
        { }
        public override void CheckForSwitch()
        {
            if(enemy_AI.IsFallingHard())
                SwitchState(EnemyStateE.falling);
            if(!enemy_AI.IsInAttackRange())
                SwitchState(EnemyStateE.idle);
        }
    }
}
