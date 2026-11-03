using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using TMPro;

public class GameStateManager : NetworkBehaviour
{
    public static GameStateManager Instance { get; private set; }

    //[Header("Game Over Config")]
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject winUI;
    [SerializeField] private TMP_Text voteText;

    private NetworkList<ulong> retryVotes = new NetworkList<ulong>();

    private NetworkVariable<bool> retryAvailable = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // just to make sure
        gameOverUI.SetActive(false);
    }

    public override void OnNetworkSpawn()
    {
        retryVotes.OnListChanged += OnRetryVotesChanged;

        UpdateVoteUI();
    }

    public void ShowGameOver()
    {
        gameOverUI.SetActive(true);

        if (IsServer)
            retryAvailable.Value = true;

        UpdateVoteUI();
    }

    public void ShowWin()
    {
        winUI.SetActive(true);

        if (IsServer)
            retryAvailable.Value = true;

        UpdateVoteUI();
    }

    // for retry button
    public void RequestRetry()
    {
        if (!IsSpawned)
            return;

        voteText.gameObject.SetActive(true);
        RetryServerRpc();
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void RetryServerRpc(RpcParams rpcParams = default)
    {
        if (!retryAvailable.Value)
            return;

        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.ConnectedClients.ContainsKey(senderId))
            return;

        if (NetworkManager.ConnectedClientsIds.Count != 2)
            return;

        // stops playerse from voting more than once
        if (retryVotes.Contains(senderId))
            return;

        retryVotes.Add(senderId);

        Debug.Log($"Player {senderId} voted to retry!");

        // Both players have voted.
        if (retryVotes.Count >= 2)
        {
            RestartLevel();
        }
    }

    private void OnRetryVotesChanged(NetworkListEvent<ulong> change)
    {
        UpdateVoteUI();
    }

    private void UpdateVoteUI()
    {
        voteText.text = $"Waiting for other players... {retryVotes.Count}/2";
    }

    private void RestartLevel()
    {
        if (!IsServer)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        retryAvailable.Value = false;

        NetworkManager.SceneManager.LoadScene(currentScene, LoadSceneMode.Single);
    }

    public override void OnNetworkDespawn()
    {
        retryVotes.OnListChanged -= OnRetryVotesChanged;
    }
}
