
using UnityEngine;

public class CameraFollower : MonoBehaviour
{
    private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed = 8f;

    private void LateUpdate()
    {
        if (target == null)
            FindLocalPlayer();

        if (target == null)
            return;

        Vector3 targetPosition = target.position + offset;

        float smoothener = 1f - Mathf.Exp(-followSpeed * Time.deltaTime);

        transform.position = Vector3.Lerp(
            transform.position,
            targetPosition,
            smoothener
        );

        transform.LookAt(target.position);
    }

    private void FindLocalPlayer()
    {
        if (PlayerRegistry.Instance == null)
            return;

        foreach (Transform player in PlayerRegistry.Instance.Players)
        {
            if (player == null)
                continue;

            PlayerRegistration registration =
                player.GetComponent<PlayerRegistration>();

            if (registration == null)
                continue;

            if (registration.IsLocallyControlled)
            {
                target = player;

                return;
            }
        }
    }
}
