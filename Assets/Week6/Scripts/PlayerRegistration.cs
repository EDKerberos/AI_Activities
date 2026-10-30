using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public class PlayerRegistration : NetworkBehaviour
{
    public bool IsLocallyControlled => IsSpawned && IsOwner;

    public override void OnNetworkSpawn()
    {
        if (PlayerRegistry.Instance != null)
        {
            PlayerRegistry.Instance.AddPlayer(transform);
        }
    }

    public override void OnNetworkDespawn()
    {
        if (PlayerRegistry.Instance != null)
        {
            PlayerRegistry.Instance.RemovePlayer(transform);
        }
    }
}
