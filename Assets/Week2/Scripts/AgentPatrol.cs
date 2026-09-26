using UnityEngine;
using UnityEngine.AI;

public class AgentPatrol : MonoBehaviour
{
    [SerializeField] private bool randomPoint;
    [SerializeField] private int currentPoint;
    [SerializeField] private int nextPoint; // <- for randomizer

    [Header("Agent Config")]
    [SerializeField] private float defaultSpeed;

    private NavMeshAgent agent; 
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = defaultSpeed;

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

        /*if (agent.remainingDistance <= agent.stoppingDistance)
        {
            MoveToNextWaypoint();
        }*/

        if (Vector3.Distance(transform.position, WaypointManager.Instance.Waypoints[currentPoint].position) <= WaypointManager.Instance.waypointRadius)
        {
            MoveToNextWaypoint();
        }

        if (!agent.SamplePathPosition(NavMesh.AllAreas, 0.1f, out NavMeshHit navHit))
        {
            if ((navHit.mask & 1) != 0)
            {
                agent.speed = defaultSpeed; // reversed for some reason
                return;
            }
            else
            {
                agent.speed = defaultSpeed * 0.5f;
            }
        }
    }

    private void MoveToNextWaypoint()
    {
        var waypoints = WaypointManager.Instance.Waypoints;

        if (!randomPoint)
        {
            currentPoint++;

            if (currentPoint >= waypoints.Count)
            {
                currentPoint = 0; // go back to point 1.
            }
        }
        else if (randomPoint)
        {
            nextPoint = Random.Range(0, waypoints.Count);

            if (nextPoint == currentPoint)
            {
                nextPoint = Random.Range(0, waypoints.Count);
            }
            else
            {
                currentPoint = nextPoint;
            }
        }

        agent.SetDestination(waypoints[currentPoint].position);
    }
}
