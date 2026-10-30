using UnityEngine;

public class SpriteBillboard : MonoBehaviour
{
    private Transform cameraTransform;

    private void LateUpdate()
    {
        if (cameraTransform == null)
        {
            if (Camera.main == null)
                return;

            cameraTransform = Camera.main.transform;
        }

        transform.rotation = cameraTransform.rotation;
    }
}
