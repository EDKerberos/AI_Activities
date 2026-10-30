
using System.Collections.Generic;
using UnityEngine;

public class PlayerRegistry : MonoBehaviour
{
    public static PlayerRegistry Instance { get; private set; }

    private readonly HashSet<Transform> players = new();

    public IEnumerable<Transform> Players => players;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public void AddPlayer(Transform player)
    {
        if (player == null)
            return;

        players.Add(player);
    }

    public void RemovePlayer(Transform player)
    {
        if (player == null)
            return;

        players.Remove(player);
    }

    private void OnDestroy()
    {
        if (Instance == this)
            Instance = null;
    }
}
