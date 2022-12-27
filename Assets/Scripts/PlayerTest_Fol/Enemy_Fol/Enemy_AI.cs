using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy_AI : MonoBehaviour
{
    public static EnemySpawner enemySpawner;
    CharacterController enemyController;
    GameObject EnemyGO;
    EnemyState enemyState;

    //------------------------------------------------------------------//
    Vector3 enemyWorldPosNew, enemyWorldPosOld;
    Vector2 enemyWorldPos2D;
    Vector3 previousDirOfMotion;
    //-----------------------------------------------------------------//
    void Start()
    {
        enemyController = GetComponent<CharacterController>();
        EnemyGO = this.gameObject;

        enemyWorldPos2D = new Vector2();
        enemyWorldPosNew = new Vector3();

        enemyState = new EnemyState();
        enemyState.Set(this, enemyController);
    }
    void Update()
    {
        if (IsOutOfRange())
        {
            return;
        }
        UpdateVariables();
        enemyState.Update();
    }
    void UpdateVariables()
    {
        enemyWorldPosOld = enemyWorldPosNew;   
        enemyWorldPosNew = EnemyGO.transform.position;
        enemyWorldPos2D.x = enemyWorldPosNew.x;
        enemyWorldPos2D.y = enemyWorldPosNew.z;
    }
    //-----------------------------------------------------------------//
    bool IsOutOfRange()
    {
        return Vector2.Distance(enemySpawner.playerWorldPos2D, enemyWorldPos2D) > enemySpawner.unloadDistance;
    }
    void AIMove(float magnitude)
    {
        Vector3 moveDirection = enemySpawner.playerWorldPos - enemyWorldPosNew;
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
        return !Physics.SphereCast(point, enemyController.radius, Vector3.down, out raycastHit, 0.1f + fallDist, enemySpawner.groundLayer);
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
        return Vector3.Distance(enemySpawner.playerWorldPos, enemyWorldPosNew) < enemySpawner.attackDistance;
    }
    bool IsInChasingDistance()
    {
        return Vector3.Distance(enemySpawner.playerWorldPos, enemyWorldPosNew) < enemySpawner.chasingDistance;
    }

    //-------------------------------------------------------------------------//
    private enum EnemyStateE
    {
        idle, chasing, falling, fallRecovery, attacking
    }

    public abstract class IEnemyState
    {
        public abstract void Action();
        public abstract void CheckForSwitch();
        public abstract void Initialize();
        public abstract void Terminate();
    }

    private class EnemyState
    {
        public Enemy_AI enemy_AI { get; private set; }
        public CharacterController enemyController { get; private set; }
        private IEnemyState[] enemyStates;
        public IEnemyState currentEnemyState { get; private set; }

        public EnemyState()
        {}

        public void Set(Enemy_AI ai, CharacterController ec)
        {
            enemy_AI = ai;
            enemyController = ec;

            enemyStates = new IEnemyState[10];
            enemyStates[0] = new ESIdle(this);
            enemyStates[1] = new ESChasing(this);
            enemyStates[2] = new ESFalling(this);
            enemyStates[3] = new ESFallRecovery(this);
            enemyStates[4] = new ESAttacking(this);

            currentEnemyState = enemyStates[0];

        }
        public void Update()
        {
            currentEnemyState.Action();
            currentEnemyState.CheckForSwitch();

            //Debug.Log("State: " + currentEnemyState);

            if (enemy_AI.IsFallingSoft())
                enemy_AI.ApplyGravity();
            else
                enemy_AI.ResetGravity();
        }
        public void SwitchState(EnemyStateE newState)
        {
            currentEnemyState.Terminate();
            currentEnemyState = enemyStates[(int)newState];
            currentEnemyState.Initialize();
        }
    }
    class ESIdle : IEnemyState
    {
        EnemyState parent;
        public ESIdle(EnemyState _parent)
        {
            parent = _parent;
        }
        public override void Action()
        {
        }
        public override void Initialize()
        { }
        public override void Terminate()
        { }
        public override void CheckForSwitch()
        {
            if(parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if(parent.enemy_AI.IsInChasingDistance())
                parent.SwitchState(EnemyStateE.chasing);
        }
    }
    class ESChasing : IEnemyState
    {
        EnemyState parent;
        bool isMoving = false;
        TimeVariant speed;
        public ESChasing(EnemyState _parent)
        {
            parent = _parent;
            speed = new TimeVariant(0, 10, 2);
        }
        public override void Action()
        {
            Debug.Log(parent);
            if (parent.enemy_AI.IsInChasingDistance())
                speed.Increment(1);
            else
                speed.Decrement(5);

            speed.Update();

            if (speed.multipleFactorNormalized == 0)
            {
                isMoving = false;
                return;
            }
            parent.enemy_AI.AIMove(speed.currentValue);
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
            if (parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if(parent.enemy_AI.IsInAttackRange())
                parent.SwitchState(EnemyStateE.attacking);
            if (!isMoving)
                parent.SwitchState(EnemyStateE.idle);
        }
    }
    class ESFalling : IEnemyState
    {
        EnemyState parent;
        const float timeSinceFallingToGoToRecovery = 1f;
        float timeSinceStart = 0;
        bool isGoingToRevocery = false;
        public ESFalling(EnemyState _parent)
        {
            parent = _parent;
        }
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
            if (!parent.enemy_AI.IsFallingHard())
            {
                if (isGoingToRevocery)
                    parent.SwitchState(EnemyStateE.fallRecovery);
                else
                    parent.SwitchState(EnemyStateE.idle);
            }
        }
    }
    class ESFallRecovery : IEnemyState
    {
        EnemyState parent;
        const float timeToRecovery = 2; //sec
        float timeSinceStart = 0;
        bool isRecovered = false;

        public ESFallRecovery(EnemyState _parent)
        {
            parent = _parent;
        }

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
            if (parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if (isRecovered)
                parent.SwitchState(EnemyStateE.idle);
        }
    }
    class ESAttacking : IEnemyState
    {
        EnemyState parent;
        public ESAttacking(EnemyState _parent)
        {
            parent = _parent;
        }
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
            if(parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if(!parent.enemy_AI.IsInAttackRange())
                parent.SwitchState(EnemyStateE.idle);
        }
    }
}


