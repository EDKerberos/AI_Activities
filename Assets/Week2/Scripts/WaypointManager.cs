using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [SerializeField] private List<Transform> groundWaypoints = new();
    [SerializeField] private List<Transform> airWaypoints = new();
    public float waypointRadius;

    public IReadOnlyList<Transform> gWaypoints => groundWaypoints;
    public IReadOnlyList<Transform> aWaypoints => airWaypoints;

    private void Awake()
    {
        Instance = this;
    }



    /* public Transform GetWaypoint(int index)
    {
        if (waypoints.Count == 0)
            return null;

        return waypoints[index % waypoints.Count];
    }*/ 
}
