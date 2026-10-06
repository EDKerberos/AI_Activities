using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AgentSpawner : MonoBehaviour
{
    [Header("Spawn Settings")]
    [SerializeField] private Transform spawnPoint;

    [Header("Agents")]
    [SerializeField] private List<AgentSpawnData> agents = new();

    private void Start()
    {
        foreach (AgentSpawnData agentData in agents)
        {
            StartCoroutine(SpawnAgents(agentData));
        }
    }

    private IEnumerator SpawnAgents(AgentSpawnData agentData)
    {
        if (agentData.agentPrefab == null)
            yield break;

        for (int i = 0; i < agentData.spawnCount; i++)
        {
            Instantiate(
                agentData.agentPrefab,
                spawnPoint.position,
                spawnPoint.rotation
            );

            yield return new WaitForSeconds(Random.Range(agentData.spawnIntervalA, agentData.spawnIntervalB));
        }
    }
}
