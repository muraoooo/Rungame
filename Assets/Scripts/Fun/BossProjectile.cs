using UnityEngine;

// Poison glob spat by the boss. Flies in an arc and kills the player on touch.
public class BossProjectile : MonoBehaviour
{
    public const float GravityScale = 1.4f;

    static Sprite globSprite;
    static Sprite landingMarkerSprite;

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
        body.gravityScale = GravityScale;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.linearVelocity = velocity;

        BossProjectile projectile = globObject.AddComponent<BossProjectile>();
        projectile.dieTime = Time.time + 4f;
        return projectile;
    }

    public static bool TryPredictLanding(Vector3 position, Vector2 velocity, out Vector3 landingPoint, out float landingTime)
    {
        const float step = 0.04f;
        const float maxSeconds = 4f;

        Vector2 previous = position;
        Vector2 simulatedVelocity = velocity;
        Vector2 gravity = Physics2D.gravity * GravityScale;

        for (float elapsed = 0f; elapsed < maxSeconds; elapsed += step)
        {
            simulatedVelocity += gravity * step;
            Vector2 next = previous + simulatedVelocity * step;

            RaycastHit2D[] hits = Physics2D.LinecastAll(previous, next);
            foreach (RaycastHit2D hit in hits)
            {
                if (hit.collider == null || hit.collider.isTrigger)
                {
                    continue;
                }

                string objectName = hit.collider.gameObject.name;
                if (!objectName.StartsWith("Ground") && !objectName.StartsWith("Platform"))
                {
                    continue;
                }

                landingPoint = new Vector3(hit.point.x, hit.point.y + 0.04f, 0f);
                landingTime = elapsed + step;
                return true;
            }

            previous = next;
        }

        landingPoint = new Vector3(previous.x, previous.y, 0f);
        landingTime = maxSeconds;
        return false;
    }

    public static void ShowLandingMarker(Vector3 position, float seconds)
    {
        GameObject marker = new GameObject("BossGlobLandingMarker");
        marker.transform.position = position;
        marker.transform.localScale = new Vector3(1.15f, 0.22f, 1f);

        SpriteRenderer renderer = marker.AddComponent<SpriteRenderer>();
        renderer.sprite = GetLandingMarkerSprite();
        renderer.color = new Color(1f, 0.08f, 0.08f, 0.36f);
        renderer.sortingOrder = 11;

        BossProjectileLandingMarker warning = marker.AddComponent<BossProjectileLandingMarker>();
        warning.lifeSeconds = seconds;
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

            RespawnSystem.KillPlayer(other.gameObject, "どくだま!");
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

    static Sprite GetLandingMarkerSprite()
    {
        if (landingMarkerSprite != null)
        {
            return landingMarkerSprite;
        }

        const int width = 64;
        const int height = 24;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((width - 1) * 0.5f, (height - 1) * 0.5f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float normalized = Mathf.Pow((x - center.x) / (width * 0.5f), 2f)
                    + Mathf.Pow((y - center.y) / (height * 0.5f), 2f);
                if (normalized > 1f)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edge = Mathf.InverseLerp(1f, 0.25f, normalized);
                texture.SetPixel(x, y, new Color(1f, 0.05f, 0.05f, Mathf.Lerp(0.08f, 0.55f, edge)));
            }
        }

        texture.Apply();
        landingMarkerSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 64f);
        return landingMarkerSprite;
    }
}

public class BossProjectileLandingMarker : MonoBehaviour
{
    public float lifeSeconds = 1f;

    SpriteRenderer spriteRenderer;
    float startTime;
    Vector3 baseScale;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startTime = Time.time;
        baseScale = transform.localScale;
    }

    void Update()
    {
        float elapsed = Time.time - startTime;
        float progress = lifeSeconds > 0f ? Mathf.Clamp01(elapsed / lifeSeconds) : 1f;
        float pulse = 1f + Mathf.Sin(Time.time * 18f) * 0.08f;
        transform.localScale = new Vector3(baseScale.x * pulse, baseScale.y, baseScale.z);

        if (spriteRenderer != null)
        {
            float alpha = Mathf.Lerp(0.38f, 0.08f, progress);
            spriteRenderer.color = new Color(1f, 0.08f, 0.08f, alpha);
        }

        if (elapsed >= lifeSeconds)
        {
            Destroy(gameObject);
        }
    }
}
