using UnityEngine;
using Unity.Netcode;

[RequireComponent(typeof(CharacterController))]
[RequireComponent(typeof(PlayerRegistration))]
public class PlayerControllerNetworking : NetworkBehaviour
{
    [SerializeField] private bool isHostPlayer;

    [Header("Movement Config")]
    [SerializeField] private float baseSpeed;
    [SerializeField] private Transform mainCamera;

    [Header("Stealth Config")]
    [SerializeField] private float stealthMultiplier;
    [SerializeField] private KeyCode stealthKey = KeyCode.LeftControl;
    
    private NetworkVariable<bool> stealthMode = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    public bool IsStealthMode => stealthMode.Value;

    [Header("Sounds Config")]
    [SerializeField] private float soundRadius;
    [SerializeField] private float footstepInterval;
    [SerializeField] private LayerMask enemyLayers;
    private float footstepTimer;

    private NetworkVariable<bool> isTrapped = new NetworkVariable<bool>(
        false,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    public bool IsTrapped => isTrapped.Value;

    private CharacterController controller;

    private void Awake()
    {
        controller = GetComponent<CharacterController>();

        mainCamera = Camera.main.transform;
    }

    public override void OnNetworkSpawn()
    {
        controller.enabled = IsOwner;
    }

    public override void OnGainedOwnership()
    {
        controller.enabled = true;
    }

    public override void OnLostOwnership()
    {
        controller.enabled = false;
    }

    private void Update()
    {
        if (!IsSpawned || !IsOwner)
            return;

        // prevents host/player 1 from
        // controlling player 2
        if (isHostPlayer && OwnerClientId != NetworkManager.ServerClientId)
            return;

        if (!isHostPlayer && OwnerClientId == NetworkManager.ServerClientId)
            return;

        // stop movement when being haunted
        if (isTrapped.Value)
        {
            footstepTimer = 0f;
            return;
        }

        // go to stealth mode mama
        stealthMode.Value = Input.GetKey(stealthKey);

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

        float speed = stealthMode.Value ? baseSpeed * stealthMultiplier : baseSpeed;

        controller.Move(moveDirection * speed * Time.deltaTime);

        footstepTimer += Time.deltaTime;

        bool isMoving = input.sqrMagnitude > 0.01f;

        if (!isMoving)
        {
            footstepTimer = 0f;
            return;
        }

        if (!stealthMode.Value)
        {
            if (footstepTimer >= footstepInterval)
            {
                MakeFootstepSounds();
            }
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Owner)]
    private void EmitFootstepServerRpc()
    {
        SoundManagerNetworking.EmitSound(transform.position, soundRadius, enemyLayers);
    }

    private void MakeFootstepSounds()
    {
        footstepTimer = 0f;
        EmitFootstepServerRpc();

        // dont forget to remove
        Debug.Log("sound made auhh.");
    }

    public void SetTrappedServer(bool trapped)
    {
        if (!IsServer)
            return;

        isTrapped.Value = trapped;
    }
}
