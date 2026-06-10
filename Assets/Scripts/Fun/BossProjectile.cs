using UnityEngine;

// Poison glob spat by the boss. Flies in an arc and kills the player on touch.
public class BossProjectile : MonoBehaviour
{
    static Sprite globSprite;

    float dieTime;
    bool exploded;

    public static BossProjectile Spawn(Vector3 position, Vector2 velocity)
    {
        GameObject globObject = new GameObject("BossGlob");
        globObject.transform.position = position;

        SpriteRenderer renderer = globObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetGlobSprite();
        renderer.sortingOrder = 13;

        CircleCollider2D collider = globObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.22f;

        Rigidbody2D body = globObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 1.4f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.linearVelocity = velocity;

        BossProjectile projectile = globObject.AddComponent<BossProjectile>();
        projectile.dieTime = Time.time + 4f;
        return projectile;
    }

    void Update()
    {
        // Menacing pulse
        float pulse = 1f + Mathf.Sin(Time.time * 14f) * 0.12f;
        transform.localScale = new Vector3(pulse, pulse, 1f);

        if (Time.time >= dieTime)
        {
            Explode();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (exploded)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            if (GameSession.HasEnded)
            {
                return;
            }

            RetroSfx.PlayGameOver();
            JuiceManager.Shake(0.5f);
            JuiceManager.Burst(other.transform.position, new Color(1f, 0.35f, 0.3f), 16, 6f);
            EndBannerUI.Show("UI/GameOverBanner");
            GameSession.EndGame(other.gameObject);
            Explode();
            return;
        }

        // Pops on the ground / platforms
        string name = other.gameObject.name;
        if (!other.isTrigger && (name.StartsWith("Ground") || name.StartsWith("Platform")))
        {
            Explode();
        }
    }

    void Explode()
    {
        if (exploded)
        {
            return;
        }

        exploded = true;
        JuiceManager.Burst(transform.position, new Color(0.7f, 0.35f, 0.95f), 7, 3.5f);
        Destroy(gameObject);
    }

    static Sprite GetGlobSprite()
    {
        if (globSprite != null)
        {
            return globSprite;
        }

        const int size = 26;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        Color core = new Color(0.72f, 0.3f, 0.95f, 1f);
        Color rim = new Color(0.4f, 0.1f, 0.6f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float radius = size * 0.46f;

                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    texture.SetPixel(x, y, distance > radius - 2.2f ? rim : core);
                }
            }
        }

        texture.Apply();
        globSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 52f);
        return globSprite;
    }
}
