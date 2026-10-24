using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AgentPatrol : MonoBehaviour
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

        if (!isAir)
            agent.speed = defaultSpeed;

        if (WaypointManager.Instance == null || agent == null)
            return;

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

            isPatroling = true;
            agent.isStopped = false;
            agent.SetDestination(Waypoints[currentPoint].position);
        }
    }

    void Update()
    {
        if (agent.pathPending)
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
            currentPoint++;

            if (currentPoint >= Waypoints.Count)
            {
                currentPoint = 0; // go back to point 1.
            }
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

        agent.SetDestination(Waypoints[currentPoint].position);
    }

    public void ResumePatrol()
    {
        isPatroling = true;

        if (Waypoints.Count > 0)
        {
            agent.isStopped = false;
            agent.SetDestination(Waypoints[currentPoint].position);
        }
    }
}
