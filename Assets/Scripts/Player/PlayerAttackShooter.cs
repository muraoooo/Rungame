using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerAttackShooter : MonoBehaviour
{
    public GameObject projectilePrefab;
    public float projectileSpeed = 9f;
    public float cooldownSeconds = 0.28f;
    public Vector2 spawnOffset = new Vector2(0.55f, 0.05f);
    public Color projectileColor = new Color(1f, 0.9f, 0.25f, 1f);

    SpriteRenderer playerSpriteRenderer;
    Sprite generatedProjectileSprite;
    float cooldownTimer;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void AddShooterToPlayer()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerAttackShooter>() == null)
        {
            player.AddComponent<PlayerAttackShooter>();
        }
    }

    void Start()
    {
        playerSpriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (!GameSession.CanControlPlayer)
        {
            return;
        }

        cooldownTimer -= Time.deltaTime;

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || cooldownTimer > 0f)
        {
            return;
        }

        if (keyboard.jKey.wasPressedThisFrame || keyboard.fKey.wasPressedThisFrame)
        {
            Shoot();
        }
    }

    void Shoot()
    {
        float facing = playerSpriteRenderer != null && playerSpriteRenderer.flipX ? -1f : 1f;
        Vector2 direction = new Vector2(facing, 0f);
        Vector3 spawnPosition = transform.position + new Vector3(spawnOffset.x * facing, spawnOffset.y, 0f);
        GameObject projectile = projectilePrefab != null
            ? Instantiate(projectilePrefab, spawnPosition, Quaternion.identity)
            : CreateDefaultProjectile(spawnPosition);

        SlimeDamagingAttack attack = projectile.GetComponent<SlimeDamagingAttack>();
        if (attack == null)
        {
            attack = projectile.AddComponent<SlimeDamagingAttack>();
        }

        attack.Fire(direction, projectileSpeed);
        cooldownTimer = cooldownSeconds;
    }

    GameObject CreateDefaultProjectile(Vector3 spawnPosition)
    {
        GameObject projectile = new GameObject("PlayerSlimeAttack");
        projectile.transform.position = spawnPosition;

        SpriteRenderer projectileRenderer = projectile.AddComponent<SpriteRenderer>();
        projectileRenderer.sprite = GetGeneratedProjectileSprite();
        projectileRenderer.color = projectileColor;
        projectileRenderer.sortingOrder = 10;

        CircleCollider2D projectileCollider = projectile.AddComponent<CircleCollider2D>();
        projectileCollider.radius = 0.12f;

        projectile.AddComponent<Rigidbody2D>();
        return projectile;
    }

    Sprite GetGeneratedProjectileSprite()
    {
        if (generatedProjectileSprite != null)
        {
            return generatedProjectileSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.InverseLerp(16f, 9f, distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        generatedProjectileSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 100f);
        return generatedProjectileSprite;
    }
}
