using UnityEngine;
using Unity.Netcode;
using TMPro;

public class ItemManager : NetworkBehaviour
{
    public static ItemManager Instance { get; private set; }

    private NetworkVariable<int> candyCount = new NetworkVariable<int>(
        0,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    [SerializeField] private TMP_Text candyCountText;
    [SerializeField] private TMP_Text lightChargeText;

    public int CandyCount => candyCount.Value;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
    }

    public override void OnNetworkSpawn()
    {
        candyCount.OnValueChanged += OnCandyCountChanged;

        UpdateCandyUI(candyCount.Value);
    }

    public override void OnNetworkDespawn()
    {
        candyCount.OnValueChanged -= OnCandyCountChanged;
    }

    public void AddCandy()
    {
        if (IsServer)
        {
            candyCount.Value++;
        }
        else
        {
            AddCandyServerRpc();
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void AddCandyServerRpc()
    {
        candyCount.Value++;
    }

    private void OnCandyCountChanged(int previousValue, int newValue)
    {
        UpdateCandyUI(newValue);
    }

    private void UpdateCandyUI(int amount)
    {
        candyCountText.text = "Candies: " + amount;
    }

    public void UpdateLightUI(int amount)
    {
        lightChargeText.text = "Shakelight Uses: " + amount;
    }
}
