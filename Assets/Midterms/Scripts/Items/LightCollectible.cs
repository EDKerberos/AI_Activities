using UnityEngine;
using Unity.Netcode;

public class LightCollectible : CollectItem
{
    protected override void OnCollected(NetworkObject player)
    {
        // grant it only to the player who collected it
        PlayerControllerNetworking playerController = player.GetComponent<PlayerControllerNetworking>();

        playerController.AddLightChargeServer();
    }
}
