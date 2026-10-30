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

    [Header("Agent Config")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private bool isAir;

    private List<Transform> Waypoints = new();

    private NavMeshAgent agent; 

    [HideInInspector] public bool isPatroling;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (WaypointManager.Instance == null || agent == null)
            return;

        if (!isAir)
            agent.speed = defaultSpeed;

        Waypoints.Clear();

        var sourceWaypoints = isAir
            ? WaypointManager.Instance.aWaypoints
            : WaypointManager.Instance.gWaypoints;

        foreach (Transform point in sourceWaypoints)
        {
            if (point != null)
                Waypoints.Add(point);
        }

        if (Waypoints.Count > 0)
        {
            currentPoint = Mathf.Clamp(currentPoint, 0, Waypoints.Count - 1);

            if (NetworkManager.Singleton == null ||
                !NetworkManager.Singleton.IsServer)
                return;

            isPatroling = true;
            agent.isStopped = false;

            SetWaypointDestination();
        }
    }

    void Update()
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        if (!isPatroling || agent == null || Waypoints.Count == 0)
            return;

        if (agent.pathPending)
            return;

        if (!agent.hasPath)
            return;

        float waypointRadius = WaypointManager.Instance != null
            ? WaypointManager.Instance.waypointRadius
            : 1f;

        float arrivalDistance = Mathf.Max(waypointRadius, agent.stoppingDistance);

        if (agent.remainingDistance <= arrivalDistance)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        if (!randomPoint)
        {
            currentPoint = (currentPoint + 1) % Waypoints.Count;
        }
        else if (randomPoint)
        {
            nextPoint = Random.Range(0, Waypoints.Count);

            if (nextPoint == currentPoint)
            {
                nextPoint = Random.Range(0, Waypoints.Count);
            }
            else
            {
                currentPoint = nextPoint;
            }
        }

        SetWaypointDestination();
    }

    public void SetWaypointDestination()
    {
        Transform target = Waypoints[currentPoint];

        agent.isStopped = false;
        agent.SetDestination(target.position);
    }

    public void ResumePatrol()
    {
        isPatroling = true;

        SetWaypointDestination();
    }
}
