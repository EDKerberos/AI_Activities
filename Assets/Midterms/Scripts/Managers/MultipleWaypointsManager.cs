using System;
using System.Collections.Generic;
using UnityEngine;

public class MultipleWaypointsManager : MonoBehaviour
{
    public static MultipleWaypointsManager Instance { get; private set; }

    [Serializable]
    public class EnemyWaypointGroup
    {
        public EnemyAINetworking enemy;
        public List<Transform> waypoints = new();
    }

    [SerializeField] private List<EnemyWaypointGroup> enemyWaypointGroups = new();

    public float waypointRadius;

    private static readonly List<Transform> emptyWaypoints = new();

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public IReadOnlyList<Transform> GetWaypointsFor(EnemyAINetworking enemy)
    {
        if (enemy == null)
            return emptyWaypoints;

        foreach (EnemyWaypointGroup group in enemyWaypointGroups)
        {
            if (group.enemy == enemy)
            {
                return group.waypoints;
            }
        }

        return emptyWaypoints;
    }

    public float GetWaypointRadius()
    {
        return waypointRadius;
    }
}
