using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using UnityEngine.AI;

public class AgentPatrol : MonoBehaviour
{
    [SerializeField] private bool randomPoint;
    [SerializeField] private int currentPoint;
    [SerializeField] private int nextPoint; // <- for randomizer

    [Header("Agent Config")]
    [SerializeField] private float defaultSpeed;
    [SerializeField] private bool isAir;

    [SerializeField] private List<Transform> Waypoints = new();
    private NavMeshAgent agent; 
    
    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        agent.speed = defaultSpeed;

        if (WaypointManager.Instance == null)
            return;

        // if (WaypointManager.Instance.Waypoints.Count == 0)
            // return;

        if (!isAir)
        {
            foreach (Transform point in WaypointManager.Instance.gWaypoints)
            {
                Waypoints.Add(point);
            }
        }
        else if (isAir)
        {
            foreach (Transform point in WaypointManager.Instance.aWaypoints)
            {
                Waypoints.Add(point);

            }
        }

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

        if (Vector3.Distance(transform.position, Waypoints[currentPoint].position) <= WaypointManager.Instance.waypointRadius)
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
}
