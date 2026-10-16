using UnityEngine;
using UnityEngine.AI;

public class EnemyAI : MonoBehaviour
{
    public enum State
    {
        //None,
        Patrol,
        Alert,
        Attack
    }

    [Header("Enemy Config")]
    [SerializeField] private float detectionRange;
    [SerializeField] private float attackRange;
    [SerializeField] private float alertTime;
    [SerializeField] private float forgetTime;
    //[SerializeField] private float attackSpeedMul;

    private float alertTimer;
    private float forgetTimer;

    private State currentState = State.Patrol;
    private Vector3 rememberPos;

    private NavMeshAgent agent;
    private AgentPatrol patrolScript;
    private Transform player;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        patrolScript = GetComponent<AgentPatrol>();

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");

        if (playerObject != null)
            player = playerObject.transform;
    }

    void Update()
    {
        if (player == null || agent == null)
            return;

        // player detection
        float distance = Vector3.Distance(transform.position, player.position);

        switch (currentState)
        {
            case State.Patrol:
                Patrol(distance);
                break;
            case State.Alert:
                // patrolScript.isPatroling = false;
                Alert(distance);
                break;
            case State.Attack:
                Attack(distance);
                break;
        }
    }

    private void Patrol(float distance)
    {
        agent.isStopped = false;

        patrolScript.isPatroling = true;

        if (distance <= detectionRange)
        {
            patrolScript.isPatroling = false;
            currentState = State.Alert;
        }
    }

    private void Alert(float distance)
    {
        agent.isStopped = true;
        agent.ResetPath();

        if (distance <= detectionRange)
        {
            // store player's position
            rememberPos = player.position;
            
            alertTimer += Time.deltaTime;
            forgetTimer = 0f;

            if (alertTimer >= alertTime)
            {
                currentState = State.Attack;
            }
        }
        else
        {
            forgetTimer += Time.deltaTime;
            alertTimer = 0f;

            if (forgetTimer >= forgetTime)
            {
                currentState = State.Patrol;
            }
        }
    }

    private void Attack(float distance)
    {
        agent.isStopped = false;
        
        // store player's position
        rememberPos = player.position;

        agent.SetDestination(player.position);
        
        if (distance > attackRange)
        {
            alertTimer = 0f;
            currentState = State.Alert;

            agent.SetDestination(rememberPos);
        }
    }
}
