using System.Collections;
using UnityEngine;
using Unity.Netcode;

public class NetworkOwnerManager : MonoBehaviour
{
    // hardcoded for two existing players,
    // if more probably convert to list
    [Header("Player Objects")]
    [SerializeField] private NetworkObject player1;
    [SerializeField] private NetworkObject player2;

    private NetworkManager networkManager;

    private IEnumerator Start()
    {
        while (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsListening)
        {
            yield return null;
        }

        networkManager = NetworkManager.Singleton;

        networkManager.OnClientConnectedCallback += OnClientConnected;
        networkManager.OnClientDisconnectCallback += OnClientDisconnected;

        if (!networkManager.IsServer)
            yield break;

        StartCoroutine(AssignHostOwnership());

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            if (clientId != NetworkManager.ServerClientId)
            {
                StartCoroutine(AssignPlayer2(clientId));
                break;
            }
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!networkManager.IsServer)
            return;

        if (clientId == NetworkManager.ServerClientId)
            return;

        StartCoroutine(AssignPlayer2(clientId));
    }

    // p1
    private IEnumerator AssignHostOwnership()
    {
        yield return new WaitUntil(() => player1 != null && player1.IsSpawned);

        if (player1.OwnerClientId != NetworkManager.ServerClientId)
        {
            player1.RemoveOwnership();
        }
    }

    // p2
    private IEnumerator AssignPlayer2(ulong clientId)
    {
        yield return new WaitUntil(() =>player2 != null && player2.IsSpawned);

        if (player2.OwnerClientId != clientId)
        {
            player2.ChangeOwnership(clientId);
        }
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (!networkManager.IsServer || player2 == null)
            return;

        if (!player2.IsSpawned)
            return;

        if (player2.OwnerClientId == clientId)
        {
            player2.RemoveOwnership();
        }
    }

    private void OnDestroy()
    {
        if (networkManager == null)
            return;

        networkManager.OnClientConnectedCallback -= OnClientConnected;
        networkManager.OnClientDisconnectCallback -= OnClientDisconnected;
    }
}
