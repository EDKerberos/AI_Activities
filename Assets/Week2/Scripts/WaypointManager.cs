using System.Collections.Generic;
using UnityEngine;

public class WaypointManager : MonoBehaviour
{
    public static WaypointManager Instance { get; private set; }

    [SerializeField]
    private List<Transform> waypoints = new();

    public IReadOnlyList<Transform> Waypoints => waypoints;

    private void Awake()
    {
        Instance = this;
    }

    public Transform GetWaypoint(int index)
    {
        if (waypoints.Count == 0)
            return null;

        return waypoints[index % waypoints.Count];
    }
}
