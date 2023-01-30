using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellNode
{
    public Vector3 position = Vector3.zero;
    public bool spawnable = false;
    public float gScore;
    public float hScore;
    public float fScore;
}

public class Enemy_AI : MonoBehaviour
{
    GameObject DebugParentGO;
    List<Vector2Int> openListGridIndex;
    List<Vector2Int> closedListGridIndex;

    public static EnemySpawner enemySpawner;
    CharacterController enemyController;
    GameObject EnemyGO;
    EnemyState enemyState;

    //------------------------------------------------------------------//
    Vector2Int ownGridIndices, parentGridIndicesNew, parentGridIndicesOld, playerOwnGridIndicesNew, playerOwnGridIndicesOld;
 
    Vector3 enemyWorldPosNew, enemyWorldPosOld;
    Vector2 enemyWorldPos2D;
    Vector3 previousDirOfMotion;
    CellNode[,] cellNodes;
    //-----------------------------------------------------------------//
    void Start()
    {
        DebugParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DebugParentGO.transform.position = Vector3.zero;

        enemyController = GetComponent<CharacterController>();
        EnemyGO = this.gameObject;

        enemyWorldPos2D = new Vector2();
        enemyWorldPosNew = new Vector3();

        enemyState = new EnemyState();
        enemyState.Set(this, enemyController);

        openListGridIndex = new List<Vector2Int>();
        closedListGridIndex = new List<Vector2Int>();

        ownGridIndices = new Vector2Int();
        parentGridIndicesNew = new Vector2Int();
        parentGridIndicesOld = new Vector2Int();
        playerOwnGridIndicesNew = new Vector2Int();
        playerOwnGridIndicesOld = new Vector2Int();

        cellNodes = new CellNode[EnemySpawner.sAIGridSize, EnemySpawner.sAIGridSize];
        for(int xIndex=0; xIndex< EnemySpawner.sAIGridSize; xIndex++)
            for(int zIndex=0; zIndex< EnemySpawner.sAIGridSize; zIndex++)
            cellNodes[xIndex, zIndex] = new CellNode();

    }

    float timeSinceLastLoaded = 0;
    const float timeBtnLoad = 5f;
    void Update()
    {

        timeSinceLastLoaded += Time.deltaTime;
        if (timeSinceLastLoaded > timeBtnLoad)
        {
            timeSinceLastLoaded = 0;
            if (IsOutOfRange())
            {
                Debug.Log("Player OutOFRange");
                return;
            }
            UpdatePositionNodes();
        }


        if(enemySpawner == null)
            return;

        UpdateVariables();
        if (IsOutOfRange())
        {
           return;
        }
        enemyState.Update();
    }
    void UpdateVariables()
    {
        parentGridIndicesOld = parentGridIndicesNew;
        playerOwnGridIndicesOld = playerOwnGridIndicesNew;
        
        enemyWorldPosOld = enemyWorldPosNew;   
        enemyWorldPosNew = EnemyGO.transform.position;
        enemyWorldPos2D.x = enemyWorldPosNew.x;
        enemyWorldPos2D.y = enemyWorldPosNew.z;
    }
    //-----------------------------------------------------------------//

    bool IsOutOfRange()
    {
        float DistBetwnPoints = (EnemySpawner.sChunkSize / EnemySpawner.sPointsPerChunk);
        float distToEdge = (DistBetwnPoints * ((EnemySpawner.sAIGridSize - 1) / 2));
        if(
            enemySpawner.playerWorldPos.x > enemyWorldPosNew.x + distToEdge ||
            enemySpawner.playerWorldPos.x < enemyWorldPosNew.x - distToEdge ||

            enemySpawner.playerWorldPos.y > enemyWorldPosNew.y + distToEdge ||
            enemySpawner.playerWorldPos.y < enemyWorldPosNew.y - distToEdge
        )
        {
            return true;
        }
        else
        {
            return false;          
        }
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
        return;
        //enemyController.Move(new Vector3(0, -gravity.currentValue, 0) * Time.deltaTime);
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
        return true;
        //return Vector3.Distance(enemySpawner.playerWorldPos, enemyWorldPosNew) < enemySpawner.chasingDistance;
    }

    //-------------------------------------------------------------------------//

    void UpdatePositionNodes()
    {
        Destroy(DebugParentGO);
        DebugParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DebugParentGO.name = "Enemy_AI_DebugParentGO";
        DebugParentGO.transform.position = Vector3.zero;


        parentGridIndicesNew = enemySpawner.GetParentIndexFromPosition(enemyWorldPosNew);

        ownGridIndices[0] = (EnemySpawner.sAIGridSize - 1) /2;
        ownGridIndices[1] = (EnemySpawner.sAIGridSize - 1) /2;


        if(parentGridIndicesNew[0] == -1 
            || parentGridIndicesNew[1] == -1 )
        {
            Debug.Log("ERROR!!!!!!!" + " : " + parentGridIndicesNew[0] + ", " + parentGridIndicesNew[1]);
        }

        for(int xIndex=0; xIndex< EnemySpawner.sAIGridSize; xIndex++)
        {
            for(int zIndex=0; zIndex< EnemySpawner.sAIGridSize; zIndex++)
            {
                int ParentIndexX = parentGridIndicesNew[0] - ((EnemySpawner.sAIGridSize - 1) / 2) + xIndex;
                int ParentIndexZ = parentGridIndicesNew[1] - ((EnemySpawner.sAIGridSize - 1) / 2) + zIndex;

                cellNodes[xIndex, zIndex].position = enemySpawner.positionNodes[ParentIndexX, ParentIndexZ].position;
                cellNodes[xIndex, zIndex].spawnable= enemySpawner.positionNodes[ParentIndexX, ParentIndexZ].spawnable;

                cellNodes[xIndex, zIndex].hScore = Vector2.Distance(cellNodes[xIndex, zIndex].position, enemySpawner.playerWorldPos2D);

                if(cellNodes[xIndex, zIndex].spawnable)
                {
                    GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.transform.parent = DebugParentGO.transform;
                    go.transform.position = cellNodes[xIndex, zIndex].position;
                    go.transform.localScale = new Vector3(2, 2, 2);
                    go.GetComponent<Collider>().enabled = false;
                    go.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.2f, 0.2f, 1.0f));
                }
            }
        }

        Debug.Log("Enemy: " + cellNodes[ownGridIndices[0], ownGridIndices[1]].position);
        GameObject go2 = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go2.transform.parent = DebugParentGO.transform;
        go2.transform.position = cellNodes[ownGridIndices[0], ownGridIndices[1]].position;
        go2.transform.localScale = new Vector3(3, 3, 3);
        go2.GetComponent<Collider>().enabled = false;
        go2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 1.0f, 0.2f, 1.0f));

        
        playerOwnGridIndicesNew = GetChildIndexFromPosition(enemySpawner.playerWorldPos);
        Debug.Log("Player: " + enemySpawner.playerWorldPos);
        GameObject go3 = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go3.transform.parent = DebugParentGO.transform;
        go3.transform.position = cellNodes[playerOwnGridIndicesNew[0], playerOwnGridIndicesNew[1]].position;
        go3.transform.localScale = new Vector3(3, 3, 3);
        go3.GetComponent<Collider>().enabled = false;
        go3.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 1.0f, 1.0f));
    }


    void UpdateAStarValues()
    {
        return;
        /*
        if(
            playerOwnGridIndicesNew[0] == playerOwnGridIndicesNew[0] &&
            playerOwnGridIndicesNew[1] == playerOwnGridIndicesNew[1] &&
            ownGridIndices[0] == ownGridIndices[0] &&
            ownGridIndices[1] == ownGridIndices[1]
        )
        {
            return;
        }
        openListGridIndex.Add(ownGridIndices);
        int[] currentNode = new int[2];
        while(true)
        {
            currentNode = FindIndexOfLowestFCost();
            break;
        }
        */

        // update variables
    }

    public Vector2Int GetChildIndexFromPosition(Vector3 position)
    {

        Vector2Int indices = new Vector2Int(-1, -1);
        float currentLowestDist = 1000f;
        for(int xIndex=0; xIndex<EnemySpawner.sAIGridSize; xIndex++)
        {
            for(int zIndex=0; zIndex<EnemySpawner.sAIGridSize; zIndex++)
            {
                float currentDist = SqDistVec3AsVec2(cellNodes[xIndex, zIndex].position, position);
                if(currentDist < currentLowestDist)
                {
                    currentLowestDist = currentDist;
                    indices[0] = xIndex;
                    indices[1] = zIndex;
                }
            }
        }
        return indices;
    }

    Vector2Int FindIndexOfLowestFCost()
    {
        Vector2Int currentLowestCostIndex = new Vector2Int();
        for(int xIndex=0; xIndex<EnemySpawner.sAIGridSize; xIndex++)
        {
            for(int zIndex=0; zIndex<EnemySpawner.sAIGridSize; zIndex++)
            {
                if(cellNodes[xIndex, zIndex].fScore < cellNodes[currentLowestCostIndex[0], currentLowestCostIndex[1]].fScore)
                {
                    currentLowestCostIndex[0] = xIndex;
                    currentLowestCostIndex[1] = zIndex;
                }
            }
        }
        return currentLowestCostIndex;
    }

    float SqDist(Vector2 a, Vector2 b)
    {
        return Mathf.Abs(Vector2.SqrMagnitude(a) - Vector2.SqrMagnitude(b));
    }
    float SqDistVec3AsVec2(Vector3 a, Vector3 b)
    {   
        return Mathf.Abs(((b.x - a.x)*(b.x - a.x)) + ((b.z - a.z)*(b.z - a.z)));
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
            //Debug.Log(parent);
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


