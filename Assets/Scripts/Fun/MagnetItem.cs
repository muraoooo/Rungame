using UnityEngine;

// Pickup that pulls nearby coins toward the player for a few seconds.
public class MagnetItem : MonoBehaviour
{
    static Sprite magnetSprite;

    Vector3 basePosition;
    bool collected;

    public static MagnetItem Spawn(Vector3 position)
    {
        GameObject itemObject = new GameObject("MagnetItem");
        itemObject.transform.position = position;

        SpriteRenderer renderer = itemObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetMagnetSprite();
        renderer.sortingOrder = 9;

        CircleCollider2D collider = itemObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.4f;

        return itemObject.AddComponent<MagnetItem>();
    }

    void Start()
    {
        basePosition = transform.position;
    }

    void Update()
    {
        transform.position = basePosition + Vector3.up * (Mathf.Sin(Time.time * 3f) * 0.14f);
        transform.localRotation = Quaternion.Euler(0f, 0f, Mathf.Sin(Time.time * 2.2f) * 12f);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (collected || !other.CompareTag("Player"))
        {
            return;
        }

        collected = true;
        ScoreSystem.ActivateMagnet(7f);
        RetroSfx.PlayFever();
        JuiceManager.Popup(transform.position + Vector3.up, "コインマグネット!!", new Color(0.45f, 0.9f, 1f), 1.2f);
        JuiceManager.Burst(transform.position, new Color(0.45f, 0.9f, 1f), 14, 5f);
        Destroy(gameObject);
    }

    static Sprite GetMagnetSprite()
    {
        if (magnetSprite != null)
        {
            return magnetSprite;
        }

        const int size = 40;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.42f);

        Color red = new Color(0.95f, 0.25f, 0.3f, 1f);
        Color tip = new Color(0.92f, 0.94f, 1f, 1f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 delta = new Vector2(x, y) - center;
                float radius = delta.magnitude;
                Color color = Color.clear;

                // Horseshoe: ring with the top quarter removed
                bool inRing = radius >= size * 0.2f && radius <= size * 0.38f;
                bool inOpening = delta.y > 0f && Mathf.Abs(delta.x) < size * 0.16f;

                if (inRing && !inOpening)
                {
                    color = red;
                }

                // White pole tips at the opening
                if (delta.y > size * 0.16f && delta.y < size * 0.4f
                    && Mathf.Abs(delta.x) >= size * 0.16f && Mathf.Abs(delta.x) <= size * 0.4f
                    && inRing)
                {
                    color = tip;
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        magnetSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 44f);
        return magnetSprite;
    }
}
