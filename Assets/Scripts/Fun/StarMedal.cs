using UnityEngine;

// Hidden collectible - three per stage, placed where only brave or skilled
// runs reach. Kept permanently only if you finish the stage (Mario star-coin
// rule), shown as a ghost once owned.
public class StarMedal : MonoBehaviour
{
    static Sprite starSprite;

    int index;
    bool collected;
    bool ownedBefore;
    Vector3 basePosition;
    float nextSparkleTime;
    SpriteRenderer spriteRenderer;

    public static StarMedal Spawn(Vector3 position, int index)
    {
        GameObject medalObject = new GameObject("StarMedal_" + index);
        medalObject.transform.position = position;

        SpriteRenderer renderer = medalObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetStarSprite();
        renderer.sortingOrder = 9;

        CircleCollider2D collider = medalObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.42f;

        StarMedal medal = medalObject.AddComponent<StarMedal>();
        medal.index = index;
        return medal;
    }

    void Start()
    {
        basePosition = transform.position;
        spriteRenderer = GetComponent<SpriteRenderer>();
        ownedBefore = ScoreSystem.IsMedalOwned(LevelManager.CurrentStage, index);

        if (spriteRenderer != null && ownedBefore)
        {
            // Ghost look for medals you already own
            spriteRenderer.color = new Color(0.6f, 0.8f, 1f, 0.5f);
        }
    }

    void Update()
    {
        float time = Time.time + index * 1.7f;
        transform.position = basePosition + Vector3.up * (Mathf.Sin(time * 2f) * 0.12f);
        float pulse = 1f + Mathf.Sin(time * 3.4f) * 0.08f;
        transform.localScale = new Vector3(pulse, pulse, 1f);

        if (!ownedBefore && Time.time >= nextSparkleTime)
        {
            nextSparkleTime = Time.time + Random.Range(0.6f, 1.2f);
            JuiceManager.Embers(transform.position + (Vector3)(Random.insideUnitCircle * 0.3f),
                new Color(1f, 0.92f, 0.5f, 0.9f), 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        ScoreSystem.CollectMedal(index, ownedBefore, transform.position);
        Destroy(gameObject);
    }

    static Sprite GetStarSprite()
    {
        if (starSprite != null)
        {
            return starSprite;
        }

        const int size = 44;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        Color gold = new Color(1f, 0.85f, 0.25f, 1f);
        Color rim = new Color(0.85f, 0.55f, 0.1f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y) - center;
                float angle = Mathf.Atan2(point.y, point.x) + Mathf.PI * 0.5f;
                float radius = size * 0.22f + size * 0.2f * Mathf.Pow(Mathf.Abs(Mathf.Cos(angle * 2.5f)), 0.6f);

                if (point.magnitude > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                }
                else
                {
                    texture.SetPixel(x, y, point.magnitude > radius - 2.5f ? rim : gold);
                }
            }
        }

        texture.Apply();
        starSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 46f);
        return starSprite;
    }
}
