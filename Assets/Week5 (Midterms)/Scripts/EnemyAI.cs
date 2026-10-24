using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        Patrol,
        Alert,
        Attack
    }

    private enum InvestigationPhase
    {
        Waiting,
        MovingToPosition,
        Searching
    }

    [Header("Enemy Config")]
    //[SerializeField] private float detectionRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float checkTime;
    [Space]
    [SerializeField] private float investigateDelay;
    [SerializeField] private float investigateDuration;

    [Header("Rotation Config")]
    [SerializeField] private float rotationSmoothness;
    [SerializeField] private float patrolTurnSpeed;

    [Header("Color Changer")]
    [SerializeField] private Material patrolColor;
    [SerializeField] private Material alertColor;
    [SerializeField] private Material attackColor;

    private float investigateTimer;

    [SerializeField] private State currentState = State.Patrol;
    private InvestigationPhase investigationPhase;
    private Vector3 rememberPos;

    private NavMeshAgent agent;
    private MeshRenderer meshRenderer;
    private AgentPatrol patrolScript;
    private EnemyVision vision;

    private Transform visiblePlayer;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        meshRenderer = GetComponent<MeshRenderer>();
        patrolScript = GetComponent<AgentPatrol>();
        vision = GetComponent<EnemyVision>();

        agent.updateRotation = false;
        agent.autoBraking = true;
    }

    void Update()
    {
        if (agent == null || vision == null || patrolScript == null)
            return;

        // player detection
        visiblePlayer = vision.VisiblePlayer;

        switch (currentState)
        {
            case State.Patrol:
                SetMaterial(patrolColor);
                Patrol();
                break;

            case State.Alert:
                SetMaterial(alertColor);
                Alert();
                break;

            case State.Attack:
                SetMaterial(attackColor);
                Attack();
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

            agent.isStopped = false;
            currentState = State.Attack;
            return;
        }

        patrolScript.isPatroling = false;

        switch (investigationPhase)
        {
            case InvestigationPhase.Waiting:
                agent.isStopped = true;
                agent.ResetPath();
                agent.velocity = Vector3.zero;

                Vector3 direction = rememberPos - transform.position;
                direction.y = 0f;

                bool facingTarget = true;

                if (direction.sqrMagnitude > 0.01f)
                {
                    Quaternion targetRotation = Quaternion.LookRotation(direction);

                    transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 180f * Time.deltaTime);

                    facingTarget = Quaternion.Angle(transform.rotation, targetRotation) < 2f;
                }

                if (facingTarget)
                {
                    investigateTimer += Time.deltaTime;
                }

                if (investigateTimer >= investigateDelay)
                {
                    investigateTimer = 0f;
                    investigationPhase = InvestigationPhase.MovingToPosition;

                    agent.isStopped = false;
                    agent.SetDestination(rememberPos);
                }

                break;

            case InvestigationPhase.MovingToPosition:
                agent.isStopped = false;
                RotateTowards(agent.desiredVelocity);

                if (agent.pathPending)
                    break;

                if (agent.pathStatus == NavMeshPathStatus.PathInvalid)
                {
                    StartSearching();
                    break;
                }

                if (agent.hasPath && agent.remainingDistance <= agent.stoppingDistance + 0.1f)
                {
                    StartSearching();
                }
                else if (!agent.hasPath)
                {
                    if (Vector3.Distance(transform.position, rememberPos) <= 1f)
                    {
                        StartSearching();
                    }
                }

                break;

            case InvestigationPhase.Searching:
            {
                agent.isStopped = true;
                investigateTimer += Time.deltaTime;

                if (investigateTimer >= investigateDuration)
                {
                    FinishInvestigation();
                }

                break;
            }
        }
    }

    private void Attack()
    {
        agent.isStopped = false;
        patrolScript.isPatroling = false;

        if (visiblePlayer == null)
        {
            BeginInvestigation();
            return;
        }

        rememberPos = visiblePlayer.position;
        agent.SetDestination(rememberPos);
    }

    private void BeginInvestigation()
    {
        investigationPhase = InvestigationPhase.Waiting;
        investigateTimer = 0f;

        patrolScript.isPatroling = false;
        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;

        currentState = State.Alert;
    }

    private void StartSearching()
    {
        investigationPhase = InvestigationPhase.Searching;
        investigateTimer = 0f;

        agent.isStopped = true;
        agent.ResetPath();
        agent.velocity = Vector3.zero;
    }

    private void FinishInvestigation()
    {
        investigateTimer = 0f;

        agent.isStopped = false;
        agent.ResetPath();

        currentState = State.Patrol;
        patrolScript.ResumePatrol();
    }

    private void SetMaterial(Material targetMaterial)
    {
        if (meshRenderer != null && targetMaterial != null)
        {
            meshRenderer.material = targetMaterial;
        }
    }

    private void RotateTowards(Vector3 direction)
    {
        direction.y = 0f;

        if (direction.sqrMagnitude < 0.01f)
            return;

        Quaternion targetRotation = Quaternion.LookRotation(direction);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, targetRotation, 360f * Time.deltaTime);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, attackRange);
    }
}
