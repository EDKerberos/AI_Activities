using UnityEngine;

[RequireComponent(typeof(CharacterController))]
public class PlayerController : MonoBehaviour
{
    [Header("Movement Config")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private Transform mainCamera;

    [Header("Stealth Config")]
    [SerializeField] private float stealthMultiplier;
    [SerializeField] private KeyCode stealthKey = KeyCode.LeftControl;
    private bool stealthMode;

    [Header("Sounds Config")]
    [SerializeField] private float soundRadius;
    [SerializeField] private float footstepInterval;
    [SerializeField] private LayerMask enemyLayers;
    private float footstepTimer;

    private CharacterController controller;

    public bool IsStealthMode => stealthMode;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();
        mainCamera = Camera.main.transform;
    }

    private void Update()
    {
        // go to stealth mode mama
        stealthMode = Input.GetKey(stealthKey);

        float horizontal = Input.GetAxisRaw("Horizontal");
        float vertical = Input.GetAxisRaw("Vertical");

        Vector3 input = new Vector3(horizontal, 0f, vertical);
        input = Vector3.ClampMagnitude(input, 1f);

        Vector3 moveDirection;

        if (mainCamera != null)
        {
            Vector3 forward = mainCamera.forward;
            Vector3 right = mainCamera.right;

            forward.y = 0f;
            right.y = 0f;

            forward.Normalize();
            right.Normalize();

            moveDirection = forward * input.z + right * input.x;
            moveDirection = Vector3.ClampMagnitude(moveDirection, 1f);
        }
        else
        {
            // fallback
            moveDirection = input;
        }

        float speed = stealthMode ? baseSpeed * stealthMultiplier : baseSpeed;

        controller.Move(moveDirection * speed * Time.deltaTime);

        footstepTimer += Time.deltaTime;

        bool isMoving = input.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        if (!stealthMode)
        {
            if (footstepTimer >= footstepInterval)
            {
                MakeFootstepSounds();
            }
        }
    }

    private void MakeFootstepSounds()
    {
        footstepTimer = 0f;
        SoundManager.EmitSound(transform.position, soundRadius, enemyLayers);

        Debug.Log("sound made auhh.");
    }
}
