
using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;

public class WinPad : NetworkBehaviour
{
    [SerializeField] private int requiredCandies;

    private HashSet<ulong> playersOnPad = new HashSet<ulong>();

    private BoxCollider winPadCollider;

    private NetworkVariable<bool> hasWon = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    private void Awake()
    {
        winPadCollider = GetComponent<BoxCollider>();
    }

    private void FixedUpdate()
    {
        if (!IsServer || !IsSpawned || hasWon.Value)
            return;

        CheckPlayersOnPad();
    }

    private void CheckPlayersOnPad()
    {
        playersOnPad.Clear();

        Vector3 localCenter = winPadCollider.center;
        Vector3 halfSize = winPadCollider.size * 0.5f;

        foreach (ulong clientId in NetworkManager.Singleton.ConnectedClientsIds)
        {
            NetworkObject player =
                NetworkManager.Singleton.ConnectedClients[clientId].PlayerObject;

            if (player == null)
            {
                continue;
            }

            Vector3 playerPosition = player.transform.position;

            Vector3 localPosition =
                winPadCollider.transform.InverseTransformPoint(playerPosition);

            bool inside =
                Mathf.Abs(localPosition.x - localCenter.x) <= halfSize.x &&
                Mathf.Abs(localPosition.z - localCenter.z) <= halfSize.z;

            if (inside)
            {
                playersOnPad.Add(clientId);
            }
        }

        //Debug.Log($"TOTAL PLAYERS ON PAD: {playersOnPad.Count}");

        CheckWinCondition();
    }

    private void CheckWinCondition()
    {
        if (!IsServer || hasWon.Value)
            return;

        if (playersOnPad.Count < 2)
            return;

        if (GameStateManager.Instance == null)
            return;

        if (ItemManager.Instance == null)
            return;

        if (ItemManager.Instance.CandyCount < requiredCandies)
            return;

        hasWon.Value = true;

        GameWinClientRpc();
    }

    [ClientRpc]
    private void GameWinClientRpc()
    {
        GameStateManager.Instance?.ShowWin();
    }
}
