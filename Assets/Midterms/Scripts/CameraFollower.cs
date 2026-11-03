
using UnityEngine;
using Unity.Netcode;

public class CameraFollower : MonoBehaviour
{
    private Transform target;

    [Header("Camera Settings")]
    [SerializeField] private Vector3 offset = new Vector3(0f, 12f, -8f);
    [SerializeField] private float followSpeed;

    private void OnEnable()
    {
        target = null;
    }

    private void LateUpdate()
    {
        if (target != null)
        {
            PlayerRegistration registration =
                target.GetComponent<PlayerRegistration>();
        }

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
        {
            return;
        }

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

                Debug.Log(
                    $"CAMERA CONNECTED | " +
                    $"Player: {player.name} | " +
                    $"Owner: {player.GetComponent<NetworkObject>().OwnerClientId}"
                );

                return;
            }
        }
    }
}
