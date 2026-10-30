using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using Unity.Netcode;

public class SoundManagerNetworking
{
    private static readonly NavMeshPath path = new NavMeshPath();

    public static void EmitSound(Vector3 soundPos, float soundRadius, LayerMask enemyLayers)
    {
        if (NetworkManager.Singleton == null || !NetworkManager.Singleton.IsServer)
            return;

        Collider[] hits = Physics.OverlapSphere(soundPos, soundRadius, enemyLayers, QueryTriggerInteraction.Ignore);

        HashSet<EnemyAINetworking> notifiedEnemies = new HashSet<EnemyAINetworking>();

        foreach (Collider hit in hits)
        {
            EnemyAINetworking enemy = hit.GetComponentInParent<EnemyAINetworking>();

            if (enemy == null || !notifiedEnemies.Add(enemy))
                continue;

            NavMeshAgent agent = enemy.GetComponent<NavMeshAgent>();

            if (agent == null || !agent.isOnNavMesh)
                continue;

            if (!NavMesh.SamplePosition(soundPos, out NavMeshHit soundHit, 2f, agent.areaMask))
            {
                continue;
            }

            if (!NavMesh.CalculatePath(agent.nextPosition, soundHit.position, agent.areaMask, path))
            {
                continue;
            }

            if (path.status != NavMeshPathStatus.PathComplete)
                continue;

            float pathDistance = 0f;

            Vector3[] corners = path.corners;

            for (int i = 1; i < corners.Length; i++)
            {
                pathDistance += Vector3.Distance(corners[i - 1], corners[i]);
            }

            if (pathDistance <= soundRadius)
            {
                enemy.HearSound(soundPos);
            }
        }
    }
}
