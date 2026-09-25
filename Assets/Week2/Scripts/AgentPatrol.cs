using UnityEngine;
using UnityEngine.AI;

public class AgentPatrol : MonoBehaviour
{
    private int currentPoint;

    private NavMeshAgent agent;
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        if (WaypointManager.Instance == null)
            return;

        if (WaypointManager.Instance.Waypoints.Count == 0)
            return;

        MoveToNextWaypoint();
    }

    void Update()
    {
        if (agent.pathPending)
            return;

        if (agent.remainingDistance <= agent.stoppingDistance)
        {
            MoveToNextWaypoint();
        }
    }

    private void MoveToNextWaypoint()
    {
        var waypoints = WaypointManager.Instance.Waypoints;

        currentPoint++;

        if (currentPoint >= waypoints.Count)
        {
            currentPoint = 0; // go back to point 1.
        }

        agent.SetDestination(waypoints[currentPoint].position);
    }
}
