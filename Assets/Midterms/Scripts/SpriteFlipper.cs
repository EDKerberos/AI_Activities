using UnityEngine;

public class SpriteFlipper : MonoBehaviour
{
    [SerializeField] private SpriteRenderer spriteRenderer;
    [SerializeField] private float directionThreshold;

    private Vector3 previousPosition;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        previousPosition = transform.position;
    }

    private void LateUpdate()
    {
        if (spriteRenderer == null)
            return;

        float horizontalMovement = transform.position.x - previousPosition.x;

        // likely temporary
        if (horizontalMovement > directionThreshold)
        {
            spriteRenderer.flipX = true;
        }
        else if (horizontalMovement < -directionThreshold)
        {
            spriteRenderer.flipX = false;
        }

        previousPosition = transform.position;
    }
}
