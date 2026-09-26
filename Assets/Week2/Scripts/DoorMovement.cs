using UnityEngine;
using Unity.AI.Navigation;

public class DoorMovement : MonoBehaviour
{
    [Header("Door Config")]
    [SerializeField] private float openHeight;
    [SerializeField] private float speed;
    [SerializeField] private float moveInterval;
    [Space]
    [SerializeField] private NavMeshSurface surfaceMesh; // temp
    [SerializeField] private bool isNotObstacle;

    private Vector3 closedPosition;
    private Vector3 openPosition;

    private bool opening;
    private float intervalTimer;

    private void Start()
    {
        closedPosition = transform.position;
        openPosition = closedPosition + Vector3.up * openHeight;
    }

    private void Update()
    {
        Vector3 target;

        if (opening)
        {
            target = openPosition;
        }
        else
        {
            target = closedPosition;
        }

        transform.position = Vector3.MoveTowards(
            transform.position,
            target,
            speed * Time.deltaTime
        );

        if (Vector3.Distance(transform.position, target) < 0.01f)
        {
            if (isNotObstacle)
            {
                UpdateSurface();
            }
            
            intervalTimer += Time.deltaTime;

            if (intervalTimer >= moveInterval)
            {
                opening = !opening;
                intervalTimer = 0f;
            }
        }
    }

    private void UpdateSurface()
    {
        surfaceMesh.UpdateNavMesh(surfaceMesh.navMeshData);
    }
}
