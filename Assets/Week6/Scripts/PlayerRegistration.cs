using UnityEngine;

public class PlayerRegistration : MonoBehaviour
{
    private void OnEnable()
    {
        if (PlayerRegistry.Instance != null)
            PlayerRegistry.Instance.AddPlayer(transform);
    }

    private void Start()
    {
        if (PlayerRegistry.Instance != null)
            PlayerRegistry.Instance.AddPlayer(transform);
    }

    private void OnDisable()
    {
        if (PlayerRegistry.Instance != null)
            PlayerRegistry.Instance.RemovePlayer(transform);
    }
}
