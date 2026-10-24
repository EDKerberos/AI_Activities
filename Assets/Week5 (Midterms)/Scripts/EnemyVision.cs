using UnityEngine;

public class EnemyVision : MonoBehaviour
{
    [Header("Vision")]
    [SerializeField] private Transform eyePoint;
    [SerializeField] private float viewDistance = 10f;
    [SerializeField, Range(0f, 360f)]
    private float fieldOfView = 90f;

    [Header("Layers")]
    [SerializeField] private LayerMask playerMask;
    [SerializeField] private LayerMask obstructionMask;

    public Transform VisiblePlayer { get; private set; }

    private void Update()
    {
        VisiblePlayer = FindVisiblePlayer();
    }

    private Transform FindVisiblePlayer()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.5f;

        Collider[] nearbyPlayers = Physics.OverlapSphere(
            origin,
            viewDistance,
            playerMask,
            QueryTriggerInteraction.Ignore
        );

        Transform bestTarget = null;
        float nearestDistance = float.MaxValue;

        foreach (Collider playerCollider in nearbyPlayers)
        {
            Transform candidate = playerCollider.transform;

            // Resolve colliders on player child objects.
            PlayerRegistration registration =
                candidate.GetComponentInParent<PlayerRegistration>();

            if (registration == null)
                continue;

            candidate = registration.transform;

            if (candidate == transform ||
                candidate.IsChildOf(transform))
                continue;

            Vector3 targetPoint =
                playerCollider.ClosestPoint(origin);

            Vector3 direction = targetPoint - origin;

            if (direction.sqrMagnitude < 0.001f)
                continue;

            // Ignore vertical differences for the FOV angle.
            Vector3 flatForward = Vector3.ProjectOnPlane(
                transform.forward, Vector3.up);

            Vector3 flatDirection = Vector3.ProjectOnPlane(
                direction, Vector3.up);

            if (flatForward.sqrMagnitude < 0.001f ||
                flatDirection.sqrMagnitude < 0.001f)
                continue;

            float angle = Vector3.Angle(
                flatForward, flatDirection);

            if (angle > fieldOfView * 0.5f)
                continue;

            // True when a wall or another obstruction blocks vision.
            if (Physics.Raycast(
                origin,
                direction.normalized,
                direction.magnitude,
                obstructionMask,
                QueryTriggerInteraction.Ignore))
                continue;

            float distance = direction.sqrMagnitude;

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                bestTarget = candidate;
            }
        }

        return bestTarget;
    }

    private void OnDrawGizmosSelected()
    {
        Vector3 origin = eyePoint != null
            ? eyePoint.position
            : transform.position + Vector3.up * 1.5f;

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(origin, viewDistance);

        Vector3 forward = Vector3.ProjectOnPlane(
            transform.forward, Vector3.up).normalized;

        Vector3 left = Quaternion.Euler(
            0f, -fieldOfView * 0.5f, 0f) * forward;

        Vector3 right = Quaternion.Euler(
            0f, fieldOfView * 0.5f, 0f) * forward;

        Gizmos.color = Color.cyan;
        Gizmos.DrawRay(origin, left * viewDistance);
        Gizmos.DrawRay(origin, right * viewDistance);
    }
}
