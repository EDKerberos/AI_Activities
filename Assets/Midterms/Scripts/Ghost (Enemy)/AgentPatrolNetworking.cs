using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class AgentPatrolNetworking : MonoBehaviour
{
    [Header("Patrol Config")]
    [SerializeField] private bool randomPoint;
    [SerializeField] private int currentPoint;
    [SerializeField] private int nextPoint;

    private int patrolDirection = 1;

    [Header("Agent Config")]
    [SerializeField] private float defaultSpeed;
    //[SerializeField] private bool isAir;

    private List<Transform> Waypoints = new();

    private NavMeshAgent agent;
    private EnemyAINetworking enemyAI;

    [HideInInspector] public bool isPatroling;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        enemyAI = GetComponent<EnemyAINetworking>();

        if (MultipleWaypointsManager.Instance == null || agent == null || enemyAI == null)
            return;

        agent.speed = defaultSpeed;

        var sourceWaypoints = MultipleWaypointsManager.Instance.GetWaypointsFor(enemyAI);

        Waypoints.Clear();

        foreach (Transform point in sourceWaypoints)
        {
            if (point != null)
                Waypoints.Add(point);
        }

        if (Waypoints.Count == 0)
        {
            Debug.LogWarning($"{gameObject.name} is missing waypoints");
            return;
        }

        currentPoint = Mathf.Clamp(currentPoint, 0, Waypoints.Count - 1);

        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        isPatroling = true;
        agent.isStopped = false;

        SetWaypointDestination();
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        if (!isPatroling || agent == null || Waypoints.Count == 0)
            return;

        if (agent.pathPending || !agent.hasPath)
            return;

        float waypointRadius = WaypointManager.Instance != null ? WaypointManager.Instance.waypointRadius : 1f;

        float arrivalDistance = Mathf.Max(waypointRadius, agent.stoppingDistance);

        if (agent.remainingDistance <= arrivalDistance)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (randomPoint)
        {
            if (Waypoints.Count <= 1)
                return;

            int nextPoint;

            do
            {
                nextPoint = Random.Range(0, Waypoints.Count);

            } while (nextPoint == currentPoint);

            currentPoint = nextPoint;
        }
        else
        {
            currentPoint += patrolDirection;

            // ping-pong patrol
            if (currentPoint >= Waypoints.Count - 1)
            {
                currentPoint = Waypoints.Count - 1;
                patrolDirection = -1;
            }
            else if (currentPoint <= 0)
            {
                currentPoint = 0;
                patrolDirection = 1;
            }
        }

        SetWaypointDestination();
    }

    public void SetWaypointDestination()
    {
        if (Waypoints.Count == 0 || agent == null)
            return;

        currentPoint = Mathf.Clamp(currentPoint, 0, Waypoints.Count - 1);

        Transform target = Waypoints[currentPoint];

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void ResumePatrol()
    {
        if (Waypoints.Count == 0 || agent == null)
            return;

        isPatroling = true;

        SetWaypointDestination();
    }
}
