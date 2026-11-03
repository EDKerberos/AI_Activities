using UnityEngine;
using Unity.Netcode;

public class CandyCollectible : CollectItem
{
    protected override void OnCollected(NetworkObject player)
    {
        ItemManager.Instance.AddCandy();
    }
}
