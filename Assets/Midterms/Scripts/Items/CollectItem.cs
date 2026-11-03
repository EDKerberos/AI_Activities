using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(NetworkObject))]
public abstract class CollectItem : NetworkBehaviour
{
    [SerializeField] private float collectionRange;

    private GameObject itemVisual;
    private Collider itemCollider;

    protected bool isCollected;

    protected virtual void Awake()
    {
        itemVisual = transform.Find("2DSprite")?.gameObject;
        itemCollider = GetComponent<Collider>();
    }

    public override void OnNetworkSpawn()
    {
        isCollected = false;

        if (itemVisual != null)
            itemVisual.SetActive(true);

        if (itemCollider != null)
            itemCollider.enabled = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        if (!IsSpawned || isCollected)
            return;

        NetworkObject playerObject = other.GetComponentInParent<NetworkObject>();

        if (playerObject == null || !playerObject.IsOwner)
            return;

        CollectServerRpc(playerObject.NetworkObjectId);
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    private void CollectServerRpc(ulong playerNetworkObjectId, RpcParams rpcParams = default)
    {
        if (isCollected || !IsSpawned)
            return;

        ulong senderId = rpcParams.Receive.SenderClientId;

        if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(playerNetworkObjectId, out NetworkObject player))
            return;

        if (player.OwnerClientId != senderId)
            return;

        Collider playerCollider = player.GetComponent<Collider>();

        float distance = Vector3.Distance(transform.position, playerCollider.ClosestPoint(transform.position));

        isCollected = true;

        OnCollected(player);

        NetworkObject.Despawn(false);
    }

    protected abstract void OnCollected(NetworkObject player);

    public override void OnNetworkDespawn()
    {
        if (itemVisual != null)
            itemVisual.SetActive(false);

        if (itemCollider != null)
            itemCollider.enabled = false;
    }
    
    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, collectionRange);

        Gizmos.color = Color.green;
        Gizmos.DrawWireSphere(transform.position, 0.15f);
    }
}
