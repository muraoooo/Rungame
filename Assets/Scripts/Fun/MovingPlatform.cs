using UnityEngine;

[RequireComponent(typeof(Rigidbody2D))]
public class MovingPlatform : MonoBehaviour
{
    public float amplitude = 2.6f;
    public float period = 3.4f;

    static Sprite platformSprite;

    Rigidbody2D body;
    Vector2 origin;
    float elapsed;

    // Named with the "Platform_" prefix so the player's grounded check accepts it.
    // "Lift" in the name lets the coin spawner skip it (coins must not float
    // where the lift only sometimes is).
    public static MovingPlatform Spawn(Vector3 position, float amplitude, float period)
    {
        GameObject platformObject = new GameObject("Platform_Lift");
        platformObject.transform.position = position;

        SpriteRenderer renderer = platformObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPlatformSprite();
        renderer.sortingOrder = 7;

        BoxCollider2D collider = platformObject.AddComponent<BoxCollider2D>();
        collider.size = new Vector2(1.7f, 0.4f);

        Rigidbody2D body = platformObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Kinematic;

        MovingPlatform platform = platformObject.AddComponent<MovingPlatform>();
        platform.amplitude = amplitude;
        platform.period = period;
        return platform;
    }

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        origin = body.position;
    }

    void FixedUpdate()
    {
        elapsed += Time.fixedDeltaTime;
        // Starts smoothly from the spawn position, rising to spawn + amplitude.
        float offset = (1f - Mathf.Cos(elapsed * Mathf.PI * 2f / period)) * 0.5f * amplitude;
        body.MovePosition(new Vector2(origin.x, origin.y + offset));
    }

    static Sprite GetPlatformSprite()
    {
        Sprite stageSprite = StageTerrainStyler.LoadPlatformSprite(LevelManager.CurrentStage, true);
        if (stageSprite != null)
        {
            return stageSprite;
        }

        if (platformSprite != null)
        {
            return platformSprite;
        }

        const int width = 68;
        const int height = 16;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color wood = new Color(0.55f, 0.4f, 0.25f, 1f);
        Color grass = new Color(0.4f, 0.78f, 0.35f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int edge = Mathf.Min(x, width - 1 - x);
                if (edge < 2 && (y < 2 || y > height - 3))
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                texture.SetPixel(x, y, y >= height - 5 ? grass : wood);
            }
        }

        texture.Apply();
        platformSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0.5f), 40f);
        return platformSprite;
    }
}
