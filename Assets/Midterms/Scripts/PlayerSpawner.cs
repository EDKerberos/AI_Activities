using UnityEngine;
using Unity.Netcode;

public class PlayerSpawner : MonoBehaviour
{
    // if implementing characters, likely change this
    // to a list or something and use the ids of them corresponding
    // to the player's choice
    [SerializeField] private GameObject player1Prefab;
    [SerializeField] private GameObject player2Prefab;
    [Space]
    [SerializeField] private Transform player1SpawnPoint;
    [SerializeField] private Transform player2SpawnPoint;

    private NetworkManager networkManager;
    private bool serverInitialized;

    private void Start()
    {
        networkManager = NetworkManager.Singleton;

        if (networkManager == null)
        {
            Debug.LogError("NetworkManager not found!");
            return;
        }

        networkManager.OnServerStarted += InitializeServer;

        if (networkManager.IsServer)
        {
            InitializeServer();
        }
    }

    private void InitializeServer()
    {
        if (serverInitialized)
            return;

        serverInitialized = true;

        networkManager.OnServerStarted -= InitializeServer;

        networkManager.OnClientConnectedCallback += OnClientConnected;

        foreach (ulong clientId in networkManager.ConnectedClientsIds)
        {
            SpawnPlayer(clientId);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        if (!networkManager.IsServer)
            return;

        SpawnPlayer(clientId);
    }

    private void SpawnPlayer(ulong clientId)
    {
        if (!networkManager.IsServer)
            return;

        // Prevent duplicate spawning.
        if (networkManager.ConnectedClients[clientId].PlayerObject != null)
            return;

        GameObject prefab;
        Transform spawnPoint;

        if (clientId == NetworkManager.ServerClientId)
        {
            prefab = player1Prefab;
            spawnPoint = player1SpawnPoint;
        }
        else
        {
            prefab = player2Prefab;
            spawnPoint = player2SpawnPoint;
        }

        GameObject player = Instantiate(prefab, spawnPoint.position, spawnPoint.rotation);

        NetworkObject networkObject = player.GetComponent<NetworkObject>();

        if (networkObject == null)
        {
            Destroy(player);
            return;
        }

        networkObject.SpawnAsPlayerObject(clientId);

        //StartCoroutine(CheckPosition(player));
    }

    private void OnDestroy()
    {
        if (networkManager != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            networkManager.OnServerStarted -= InitializeServer;
        }
    }
}
