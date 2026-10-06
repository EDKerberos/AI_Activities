using UnityEngine;
using UnityEngine.AI;

public class FlyerMovementMod : MonoBehaviour
{
    [Header("Flight Config")]
    [SerializeField] private float flightHeight;
    [SerializeField] private float verticalSmooth;

    private NavMeshAgent agent;
    private float currentHeight;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        currentHeight = flightHeight;
        agent.baseOffset = currentHeight;
    }

    void Update()
    {
        if (!NavMesh.SamplePosition(transform.position, out NavMeshHit hit, 3f, NavMesh.AllAreas))
        {
            return;
        }

        float targetHeight = flightHeight;

        currentHeight = Mathf.Lerp(
            currentHeight,
            targetHeight,
            verticalSmooth * Time.deltaTime
        );

        agent.baseOffset = currentHeight;
    }
}
