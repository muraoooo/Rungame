using UnityEngine;

public class CheckpointFlag : MonoBehaviour
{
    static Sprite flagSprite;

    SpriteRenderer spriteRenderer;
    bool activated;

    public static CheckpointFlag Spawn(Vector2 groundPoint)
    {
        GameObject flagObject = new GameObject("CheckpointFlag");
        flagObject.transform.position = new Vector3(groundPoint.x, groundPoint.y, 0f);

        SpriteRenderer renderer = flagObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetFlagSprite();
        renderer.sortingOrder = 6;
        renderer.color = new Color(0.75f, 0.78f, 0.85f, 1f);

        BoxCollider2D collider = flagObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.6f, 2.2f);
        collider.offset = new Vector2(0f, 1.1f);

        return flagObject.AddComponent<CheckpointFlag>();
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (activated || !other.CompareTag("Player"))
        {
            return;
        }

        activated = true;
        RespawnSystem.SetCheckpoint(transform.position);
        RetroSfx.PlayCoin();
        JuiceManager.Popup(transform.position + Vector3.up * 2.2f, "チェックポイント!", new Color(0.5f, 1f, 0.6f), 1.1f);
        JuiceManager.Burst(transform.position + Vector3.up * 1.5f, new Color(0.5f, 1f, 0.6f), 10, 4f);

        if (spriteRenderer != null)
        {
            spriteRenderer.color = Color.white;
        }
    }

    static Sprite GetFlagSprite()
    {
        if (flagSprite != null)
        {
            return flagSprite;
        }

        const int width = 30;
        const int height = 56;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color pole = new Color(0.5f, 0.45f, 0.4f, 1f);
        Color flag = new Color(0.35f, 0.9f, 0.5f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = Color.clear;

                // Pole on the left
                if (x >= 4 && x <= 7)
                {
                    color = pole;
                }

                // Triangular flag at the top
                int fromTop = height - 1 - y;
                if (fromTop >= 3 && fromTop <= 17)
                {
                    float reach = 8f + (width - 12) * (1f - Mathf.Abs(fromTop - 10) / 8f);
                    if (x > 7 && x < reach)
                    {
                        color = flag;
                    }
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        flagSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.2f, 0f), 26f);
        return flagSprite;
    }
}
