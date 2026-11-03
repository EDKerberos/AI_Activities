using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class SpriteRandomizer : NetworkBehaviour
{
    [SerializeField] private List<Sprite> sprites = new List<Sprite>();

    private SpriteRenderer spriteRenderer;

    private NetworkVariable<int> spriteIndex = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
    );

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    public override void OnNetworkSpawn()
    {
        spriteIndex.OnValueChanged += OnSpriteChanged;

        if (IsServer && sprites.Count > 0)
        {
            spriteIndex.Value = Random.Range(0, sprites.Count);
        }

        UpdateSprite(spriteIndex.Value);
    }

    private void OnSpriteChanged(int previous, int current)
    {
        UpdateSprite(current);
    }

    private void UpdateSprite(int index)
    {
        if (spriteRenderer == null)
            return;

        if (index < 0 || index >= sprites.Count)
            return;

        spriteRenderer.sprite = sprites[index];
    }

    public override void OnNetworkDespawn()
    {
        spriteIndex.OnValueChanged -= OnSpriteChanged;
    }
}
