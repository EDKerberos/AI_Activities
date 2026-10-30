using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class EnemyAINetworking : NetworkBehaviour
{
    private enum State
    {
        Patrol,
        Alert,
        Attack,
        Attacking
    }

    private enum InvestigationPhase
    {
        Waiting,
        MovingToPosition,
        Searching
    }

    [Header("Enemy Config")]
    [SerializeField] private float attackRange;
    [SerializeField] private float killTime;

    private float killTimer;
    private PlayerControllerNetworking trappedPlayer;

    [Header("Search Config")]
    [SerializeField] private float investigateDelay;
    [SerializeField] private float searchRotationSpeed;
    [SerializeField] private float searchRotations;

    private float searchStartAngle;
    private float searchRotatedAngle;

    private float investigateTimer;
    private bool investigationDestinationSet;

    private float attackTimer;

    /*[Header("Color Changer")]
    [SerializeField] private Material patrolColor;
    [SerializeField] private Material alertColor;
    [SerializeField] private Material attackColor;*/

    private State currentState = State.Patrol;
    private InvestigationPhase investigationPhase;
    private Vector3 rememberPos;

    private NavMeshAgent agent;
    //private MeshRenderer meshRenderer;
    private AgentPatrolNetworking patrolScript;
    private EnemyVisionNetworking vision;

    private Transform visiblePlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        //meshRenderer = GetComponent<MeshRenderer>();
        patrolScript = GetComponent<AgentPatrolNetworking>();
        vision = GetComponent<EnemyVisionNetworking>();
    }       

    void Update()
    {
        if (!IsSpawned || !IsServer)
            return;

        if (agent == null || vision == null || patrolScript == null)
            return;

        // player detection
        visiblePlayer = vision.VisiblePlayer;

        switch (currentState)
        {
            case State.Patrol:
                //SetMaterial(patrolColor);
                Patrol();
                break;

            case State.Alert:
                //SetMaterial(alertColor);
                Alert();
                break;

            case State.Attack:
                //SetMaterial(attackColor);
                Attack();
                break;
            case State.Attacking:
                //SetMaterial(attackColor);
                Attacking();
                break;
        }
    }

    private void Patrol()
    {
        agent.isStopped = false;

        if (visiblePlayer != null)
        {
            patrolScript.isPatroling = false;

            rememberPos = visiblePlayer.position;
            currentState = State.Attack;
            return;
        }

        if (!patrolScript.isPatroling)
        {
            patrolScript.ResumePatrol();
        }
    }

    private void Alert()
    {
        if (visiblePlayer != null)
        {
            rememberPos = visiblePlayer.position;
            currentState = State.Attack;
            return;
        }

        patrolScript.isPatroling = false;

        switch (investigationPhase)
        {
            case InvestigationPhase.Waiting:
            {
                agent.isStopped = true;

                investigateTimer += Time.deltaTime;

                if (investigateTimer >= investigateDelay)
                {
                    investigateTimer = 0f;
                    investigationPhase = InvestigationPhase.MovingToPosition;

                    agent.isStopped = false;

                    investigationDestinationSet = agent.SetDestination(rememberPos);
                }

                break;
            }

            case InvestigationPhase.MovingToPosition:
            {
                agent.isStopped = false;

                if (agent.pathPending)
                    break;

                if (!agent.hasPath)
                {
                    if (Vector3.Distance(transform.position, rememberPos) <= Mathf.Max(agent.stoppingDistance, 0.5f))
                    {
                        StartSearching();
                    }

                    break;
                }

                if (agent.pathStatus == NavMeshPathStatus.PathInvalid || agent.pathStatus == NavMeshPathStatus.PathPartial)
                {
                    investigationDestinationSet = false;
                    break;
                }

                if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.15f)
                {
                    StartSearching();
                }

                break;
            }

            case InvestigationPhase.Searching:
            {
                agent.isStopped = true;

                float rotationThisFrame = searchRotationSpeed * Time.deltaTime;

                transform.Rotate(0f, rotationThisFrame, 0f, Space.World);

                searchRotatedAngle += rotationThisFrame;

                // how many full search rotations are needed
                // before the search is finished; full 360 = 1 rotation
                float requiredRotation = 360f * searchRotations;

                if (searchRotatedAngle >= requiredRotation)
                {
                    FinishInvestigation();
                }

                break;
            }
        }
    }

    private void Attack()
    {
        patrolScript.isPatroling = false;

        if (visiblePlayer == null)
        {
            BeginInvestigation();
            return;
        }

        agent.isStopped = false;

        rememberPos = visiblePlayer.position;
        agent.SetDestination(rememberPos);

        float distance = Vector3.Distance(
        transform.position,
        visiblePlayer.position
        );

        if (distance <= attackRange)
        {
            StartAttacking();
        }
    }

    private void StartAttacking()
    {
        if (visiblePlayer == null)
            return;

        trappedPlayer = visiblePlayer.GetComponentInParent<PlayerControllerNetworking>();

        agent.isStopped = true;
        agent.ResetPath();

        trappedPlayer.SetTrappedServer(true);

        killTimer = 0f;

        // hide the fovVisual
        vision.SetFOVVisible(false);

        currentState = State.Attacking;
    }

    private void Attacking()
    {
        agent.isStopped = true;

        if (trappedPlayer == null)
            return;

        // to improve; for when player gets freed
        if (!trappedPlayer.IsTrapped)
        {
            trappedPlayer = null;

            // show the fovVisual again
            vision.SetFOVVisible(true);

            if (visiblePlayer != null)
            {
                currentState = State.Attack;
            }
            else
            {
                BeginInvestigation();
            }

            return;
        }


        killTimer += Time.deltaTime;

        if (killTimer >= killTime)
        {
            GameOverClientRpc();
        }
    }

    private void BeginInvestigation()
    {
        investigationPhase = InvestigationPhase.Waiting;
        investigateTimer = 0f;
        investigationDestinationSet = false;

        patrolScript.isPatroling = false;
        agent.isStopped = true;
        agent.ResetPath();

        currentState = State.Alert;
    }

    private void StartSearching()
    {
        investigationPhase = InvestigationPhase.Searching;
        investigateTimer = 0f;
        investigationDestinationSet = false;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        // save current facing direction
        searchStartAngle = transform.eulerAngles.y;
        searchRotatedAngle = 0f;
    }

    private void FinishInvestigation()
    {
        investigateTimer = 0f;
        investigationDestinationSet = false;

        agent.isStopped = false;
        agent.ResetPath();

        currentState = State.Patrol;
        patrolScript.ResumePatrol();
    }

    /*private void SetMaterial(Material targetMaterial)
    {
        if (meshRenderer != null && targetMaterial != null)
        {
            meshRenderer.material = targetMaterial;
        }
    }*/

    public void HearSound(Vector3 soundPos)
    {
        if (currentState == State.Attack)
            return;

        rememberPos = soundPos;

        BeginInvestigation();
    }

    [ClientRpc]
    private void GameOverClientRpc()
    {
        GameStateManager.Instance?.ShowGameOver();
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
