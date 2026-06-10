using UnityEngine;

public class Spike : MonoBehaviour
{
    static Sprite spikeSprite;

    public static Spike Spawn(Vector2 groundPoint, float width)
    {
        GameObject spikeObject = new GameObject("Spike");
        spikeObject.transform.position = new Vector3(groundPoint.x, groundPoint.y, 0f);
        spikeObject.transform.localScale = new Vector3(width, 1f, 1f);

        SpriteRenderer renderer = spikeObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSpikeSprite();
        renderer.sortingOrder = 8;

        BoxCollider2D collider = spikeObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.86f, 0.26f);
        collider.offset = new Vector2(0f, 0.16f);

        return spikeObject.AddComponent<Spike>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryKill(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryKill(other);
    }

    void TryKill(Collider2D other)
    {
        if (GameSession.HasEnded || !other.CompareTag("Player"))
        {
            return;
        }

        RespawnSystem.KillPlayer(other.gameObject);
    }

    static Sprite GetSpikeSprite()
    {
        if (spikeSprite != null)
        {
            return spikeSprite;
        }

        const int width = 64;
        const int height = 26;
        const int teeth = 4;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color steel = new Color(0.78f, 0.8f, 0.85f, 1f);
        Color shadow = new Color(0.45f, 0.47f, 0.55f, 1f);
        float toothWidth = (float)width / teeth;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float local = Mathf.Repeat(x, toothWidth) / toothWidth;
                float peak = 1f - Mathf.Abs(local - 0.5f) * 2f;
                bool inside = y < peak * (height - 1);

                if (!inside)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                texture.SetPixel(x, y, local < 0.5f ? steel : shadow);
            }
        }

        texture.Apply();
        spikeSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 64f);
        return spikeSprite;
    }
}
