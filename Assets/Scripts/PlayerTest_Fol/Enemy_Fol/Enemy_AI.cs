using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CellNode
{
    public CellNode parent;
    public int xIndex = -1, zIndex = -1;
    public Vector3 position = new Vector3(-1f, -1f, -1f);
    public bool spawnable = false;
    public float gScore = 0;
    public float hScore = 0;
    public float GetFScore() { return gScore + hScore; }

}

public class Enemy_AI : MonoBehaviour
{

    static bool DebugShouldShowAIGrid = false;
    static bool DebugShouldShowAIPath = true;
    GameObject DebugParentGO;
    public static EnemySpawner enemySpawner;
    CharacterController enemyController;
    GameObject EnemyGO;
    EnemyState enemyState;

    //------------------------------------------------------------------//
    public bool isOutOfRange = false;
    List<CellNode> enemyPath;
    int currentPathIndex = 0;
    CellNode enemyCellNode, playerCellNodeOld, playerCellNodeNew;
    ParentCellNode enemyParentCellNodeNew, enemyParentCellNodeOld, playerParentCellNodeNew, playerParentCellNodeOld;
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
        
        enemyPath = new List<CellNode>();

        cellNodes = new CellNode[EnemySpawner.sAIGridSize, EnemySpawner.sAIGridSize];
        for (int xIndex = 0; xIndex < EnemySpawner.sAIGridSize; xIndex++)
        {
            for (int zIndex = 0; zIndex < EnemySpawner.sAIGridSize; zIndex++)
            {
                cellNodes[xIndex, zIndex] = new CellNode();
            }
        }
    }

    float timeSinceLastLoaded = 0;
    const float timeBtnLoad = 5f;
    void Update()
    {

        timeSinceLastLoaded += Time.deltaTime;
        if (timeSinceLastLoaded > timeBtnLoad)
        {
            timeSinceLastLoaded = 0;
         
            if (IsPlayerOutOfRange() || enemySpawner.IsEnemyOutOfRange(enemyWorldPosNew))
            {
                Debug.Log("Enemy Or Player OutOFRange");
                isOutOfRange = true;
                return;
            }
            isOutOfRange = false;
            if(ShouldPathUpdated())
            {
                GetPositionNodesFromParent();
                UpdateAStarValues();
            }
        }

        if (enemySpawner == null)
            return;

        UpdateVariables();
 
        enemyState.Update();
    }

    bool ShouldPathUpdated()
    {
        enemyParentCellNodeNew = enemySpawner.GetParentIndexFromPosition(enemyWorldPosNew);
        playerParentCellNodeNew = enemySpawner.GetParentIndexFromPosition(enemySpawner.playerWorldPos);

        if(
            playerParentCellNodeNew == playerParentCellNodeOld &&
            enemyParentCellNodeNew == enemyParentCellNodeOld
        )
        {
            Debug.Log("Path NOT updated!");
            return false;
        }
        Debug.Log("Path updated!");
        playerParentCellNodeOld = playerParentCellNodeNew;
        enemyParentCellNodeOld = enemyParentCellNodeNew;
        return true;

    }
    void UpdateVariables()
    {
        playerCellNodeOld = playerCellNodeNew;

        enemyWorldPosOld = enemyWorldPosNew;
        enemyWorldPosNew = EnemyGO.transform.position;

        enemyWorldPos2D.x = enemyWorldPosNew.x;
        enemyWorldPos2D.y = enemyWorldPosNew.z;
    }

    //-----------------------------------------------------------------//



    bool IsPlayerOutOfRange()
    {
        float DistBetwnPoints = (EnemySpawner.sChunkSize / EnemySpawner.sPointsPerChunk);
        float distToEdge = (DistBetwnPoints * ((EnemySpawner.sAIGridSize - 1) / 2));
        if (
            enemySpawner.playerWorldPos.x > enemyWorldPosNew.x + distToEdge ||
            enemySpawner.playerWorldPos.x < enemyWorldPosNew.x - distToEdge ||

            enemySpawner.playerWorldPos.z > enemyWorldPosNew.z + distToEdge ||
            enemySpawner.playerWorldPos.z < enemyWorldPosNew.z - distToEdge
        )
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    static float ThresholdDistance = 0.2f;
    void AIMove(float magnitude)
    {
        if(enemyPath.Count < 1)
        {
            //Debug.Log("Path not created yet!");
            return;
        }

        if(Vec3AsVec2Dist(enemyWorldPosNew, enemyPath[currentPathIndex].position) < ThresholdDistance)
        {
            currentPathIndex++;
        }
        if(currentPathIndex == enemyPath.Count-1)
        {
            //Debug.Log("Reached Player");
            return;
        }

        Vector3 moveDirection = enemyPath[currentPathIndex].position - enemyWorldPosNew;
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
        return !isOutOfRange;
        //return Vector3.Distance(enemySpawner.playerWorldPos, enemyWorldPosNew) < enemySpawner.chasingDistance;
    }

    //-------------------------------------------------------------------------//

    void GetPositionNodesFromParent()
    {
        DebugDestroyParentGO();

        for (int xIndex = 0; xIndex < EnemySpawner.sAIGridSize; xIndex++)
        {
            for (int zIndex = 0; zIndex < EnemySpawner.sAIGridSize; zIndex++)
            {
                int ParentIndexX = enemyParentCellNodeNew.xIndex - (int)((EnemySpawner.sAIGridSize - 1) / 2) + xIndex;
                int ParentIndexZ = enemyParentCellNodeNew.zIndex - (int)((EnemySpawner.sAIGridSize - 1) / 2) + zIndex;

                cellNodes[xIndex, zIndex].position = enemySpawner.parentCellNodes[ParentIndexX, ParentIndexZ].position;
                cellNodes[xIndex, zIndex].spawnable = enemySpawner.parentCellNodes[ParentIndexX, ParentIndexZ].spawnable;
                cellNodes[xIndex, zIndex].xIndex = xIndex;
                cellNodes[xIndex, zIndex].zIndex = zIndex;
                //cellNodes[xIndex, zIndex].hScore = Vector3.Distance(cellNodes[xIndex, zIndex].position, enemySpawner.playerWorldPos);
            }
        }
        enemyCellNode = cellNodes[(EnemySpawner.sAIGridSize - 1) / 2, (EnemySpawner.sAIGridSize - 1) / 2];
        playerCellNodeNew = GetChildNodeFromPosition(enemySpawner.playerWorldPos);

        if(DebugShouldShowAIGrid)
        {
            DebugShowAIGrid();
        }
        return;
    }


    void UpdateAStarValues()
    {
        List<CellNode> openCellNodesList = new List<CellNode>();
        List<CellNode> closedCellNodesList = new List<CellNode>();

        openCellNodesList.Add(enemyCellNode);

        while(openCellNodesList.Count > 0)
        {
            CellNode currentNode = FindNodeWithLowestFCost(openCellNodesList);

            openCellNodesList.Remove(currentNode);
            closedCellNodesList.Add(currentNode);

            if(currentNode == playerCellNodeNew)
            {
                ReTracePath(enemyCellNode, playerCellNodeNew);
                return;
            }

            List<CellNode> neighbours = GetNeighbours(currentNode);
            foreach(CellNode neighbour in neighbours)
            {

                if(!neighbour.spawnable || closedCellNodesList.Contains(neighbour))
                {
                    continue;
                }

                float newMovementCostToNeighbour = currentNode.gScore + GetDistanceBetweenNode(currentNode, neighbour);
                if(newMovementCostToNeighbour < neighbour.gScore || !openCellNodesList.Contains(neighbour))
                {
                    neighbour.gScore = newMovementCostToNeighbour;
                    neighbour.hScore = GetDistanceBetweenNode(neighbour, playerCellNodeNew);
                    neighbour.parent = currentNode;
                    
                    if(!openCellNodesList.Contains(neighbour))
                    {
                        openCellNodesList.Add(neighbour);
                    }
                }
            }

        }

        return;
    }

    void ReTracePath(CellNode startNode, CellNode endNode)
    {
        List<CellNode> path = new List<CellNode>();
        CellNode currentNode = endNode;
        while(currentNode != startNode)
        {
            path.Add(currentNode);
            currentNode = currentNode.parent;
        }
        path.Reverse();
        enemyPath = path;
        currentPathIndex = 0;

        if(DebugShouldShowAIPath)
        {
            DebugShowAIPath();
        }
    }
    float GetDistanceBetweenNode(CellNode a, CellNode b)
    {
        int distX = Mathf.Abs(a.xIndex - b.xIndex);
        int distZ = Mathf.Abs(a.zIndex - b.zIndex);

        if(distX > distZ)
        {
            return (EnemySpawner.sDiagonalDistBetwnPoint * distZ) + (EnemySpawner.sStraightDistBetwnPoint * (distX-distZ));
        }else
        {
            return (EnemySpawner.sDiagonalDistBetwnPoint * distX) + (EnemySpawner.sStraightDistBetwnPoint * (distZ-distX));
        }
    }

    public CellNode GetChildNodeFromPosition(Vector3 position)
    {
        int xIndexOut = -1, zIndexOut = -1;
        for(int xIndex=0; xIndex<EnemySpawner.sAIGridSize; xIndex++)
        {
            if(Mathf.Abs(position.x - cellNodes[xIndex, 0].position.x) < ((EnemySpawner.sChunkSize / EnemySpawner.sPointsPerChunk) * 0.8f))
            {
                xIndexOut = xIndex;
                break;
            }    
        }
        for(int zIndex=0; zIndex<EnemySpawner.sAIGridSize; zIndex++)
        {
            if(Mathf.Abs(position.z - cellNodes[0, zIndex].position.z) < ((EnemySpawner.sChunkSize / EnemySpawner.sPointsPerChunk) * 0.8f))
            {
                zIndexOut = zIndex;
                break;
            }
        }
        return cellNodes[xIndexOut, zIndexOut];
    }

    CellNode FindNodeWithLowestFCost(List<CellNode> openCellNodesList)
    {
        if(openCellNodesList.Count == 1)
        {
            return openCellNodesList[0];
        }
        CellNode nodeWithLowestFcost = openCellNodesList[0]; 
            foreach(CellNode node in openCellNodesList)
            {
                if (
                    node.GetFScore() < nodeWithLowestFcost.GetFScore() ||
                    (node.GetFScore() == nodeWithLowestFcost.GetFScore() && node.hScore < nodeWithLowestFcost.hScore))
                {
                    nodeWithLowestFcost = node;   
                }
            }
        return nodeWithLowestFcost;
    }

    List<CellNode> GetNeighbours(CellNode node)
    {
        List<CellNode> neighbours = new List<CellNode>();
        for (int x = -1; x <= 1; x++)
        {
            for (int z = -1; z <= 1; z++)
            {
                if(x==0 && z==0)
                {
                    continue;
                }
                int xIndex = node.xIndex + x;
                int zIndex = node.zIndex + z;
                
                if (
                    xIndex >= 0 && xIndex < EnemySpawner.sAIGridSize &&
                    zIndex >= 0 && zIndex < EnemySpawner.sAIGridSize
                )
                {
                    neighbours.Add(cellNodes[xIndex, zIndex]);
                }
            }
        }
        return neighbours;
    }
    
    float Vec3AsVec2Dist(Vector3 a, Vector3 b)
    {
        return Vector2.Distance(new Vector2(a.x, a.z), new Vector2(b.x, b.z));
    }
    //-------------------------------------------------------------------------//
    //-------------------------------------------------------------------------//
    void DebugDestroyParentGO()
    {
        Destroy(DebugParentGO);
        DebugParentGO = GameObject.CreatePrimitive(PrimitiveType.Cube);
        DebugParentGO.name = "Enemy_AI_DebugParentGO";
        DebugParentGO.transform.position = Vector3.zero;
    }
    void DebugShowAIPath()
    {
        int i=0;
        foreach(CellNode node in enemyPath)
        {
            GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"path noe: {i}";
            go.transform.parent = DebugParentGO.transform;
            go.transform.position = node.position;
            go.transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);
            go.GetComponent<Collider>().enabled = false;
            go.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 0.2f, 1.0f));
        }
    }
    void DebugShowAIGrid()
    {
        GameObject go2 = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go2.transform.parent = DebugParentGO.transform;
        go2.transform.position = enemyCellNode.position;
        go2.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
        go2.GetComponent<Collider>().enabled = false;
        go2.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 1.0f, 0.2f, 1.0f));

        GameObject go3 = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
        go3.name = "PlayerNode";
        go3.transform.parent = DebugParentGO.transform;
        go3.transform.position = playerCellNodeNew.position;
        go3.transform.localScale = new Vector3(2.2f, 2.2f, 2.2f);
        go3.GetComponent<Collider>().enabled = false;
        go3.GetComponent<Renderer>().material.SetColor("_Color", new Color(0.2f, 0.2f, 1.0f, 1.0f));

        for (int xIndex = 0; xIndex < EnemySpawner.sAIGridSize; xIndex++)
        {
            for (int zIndex = 0; zIndex < EnemySpawner.sAIGridSize; zIndex++)
            {
                if (cellNodes[xIndex, zIndex].spawnable)
                {
                    GameObject go = UnityEngine.GameObject.CreatePrimitive(PrimitiveType.Cube);
                    go.name = $"index: ({xIndex}, {zIndex})";
                    go.transform.parent = DebugParentGO.transform;
                    go.transform.position = cellNodes[xIndex, zIndex].position;
                    go.transform.localScale = new Vector3(1.5f, 1.5f, 1.5f);
                    go.GetComponent<Collider>().enabled = false;
                    go.GetComponent<Renderer>().material.SetColor("_Color", new Color(1.0f, 0.2f, 0.2f, 1.0f));
                }
            }
        }
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
        { }

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
            if (parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if (parent.enemy_AI.IsInChasingDistance())
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
            if (parent.enemy_AI.IsInAttackRange())
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
            if (parent.enemy_AI.IsFallingHard())
                parent.SwitchState(EnemyStateE.falling);
            if (!parent.enemy_AI.IsInAttackRange())
                parent.SwitchState(EnemyStateE.idle);
        }
    }
}


