using UnityEngine;

public class Coin : MonoBehaviour
{
    static Sprite coinSprite;

    float spinOffset;
    Vector3 basePosition;
    Vector3 baseScale;
    bool collected;

    public static Coin Spawn(Vector3 position)
    {
        GameObject coinObject = new GameObject("Coin");
        coinObject.transform.position = position;

        SpriteRenderer renderer = coinObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetCoinSprite();
        renderer.sortingOrder = 9;

        CircleCollider2D collider = coinObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.3f;

        return coinObject.AddComponent<Coin>();
    }

    void Start()
    {
        basePosition = transform.position;
        baseScale = transform.localScale;
        spinOffset = Random.value * 10f;
    }

    void Update()
    {
        float time = Time.time + spinOffset;

        float spin = Mathf.Cos(time * 5f);
        float spinWidth = Mathf.Sign(spin) * Mathf.Max(0.18f, Mathf.Abs(spin));
        transform.localScale = new Vector3(baseScale.x * spinWidth, baseScale.y, baseScale.z);

        transform.position = basePosition + Vector3.up * (Mathf.Sin(time * 2.4f) * 0.1f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        ScoreSystem.AddCoin(transform.position);
        RetroSfx.PlayCoin();
        JuiceManager.Burst(transform.position, new Color(1f, 0.84f, 0.25f), 9, 4.5f);
        Destroy(gameObject);
    }

    static Sprite GetCoinSprite()
    {
        if (coinSprite != null)
        {
            return coinSprite;
        }

        const int size = 32;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        Color gold = new Color(1f, 0.84f, 0.25f, 1f);
        Color rim = new Color(0.82f, 0.55f, 0.1f, 1f);
        Color shine = new Color(1f, 0.97f, 0.75f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float radius = size * 0.46f;

                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                Color color = distance > radius - 2.4f ? rim : gold;

                float shineDistance = Vector2.Distance(new Vector2(x, y), center + new Vector2(-4f, 5f));
                if (shineDistance < 4f && distance <= radius - 2.4f)
                {
                    color = shine;
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        coinSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 56f);
        return coinSprite;
    }
}
