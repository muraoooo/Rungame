using System.Collections;
using UnityEngine;

public class Spring : MonoBehaviour
{
    public float launchPower = 15f;

    static Sprite springSprite;

    float nextLaunchTime;
    Vector3 baseScale;

    public static Spring Spawn(Vector2 groundPoint)
    {
        GameObject springObject = new GameObject("Spring");
        springObject.transform.position = new Vector3(groundPoint.x, groundPoint.y, 0f);

        SpriteRenderer renderer = springObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetSpringSprite();
        renderer.sortingOrder = 8;

        BoxCollider2D collider = springObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.7f, 0.4f);
        collider.offset = new Vector2(0f, 0.2f);

        return springObject.AddComponent<Spring>();
    }

    void Start()
    {
        baseScale = transform.localScale;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (Time.time < nextLaunchTime || !other.CompareTag("Player"))
        {
            return;
        }

        Rigidbody2D playerBody = other.GetComponent<Rigidbody2D>();
        if (playerBody == null)
        {
            return;
        }

        nextLaunchTime = Time.time + 0.3f;
        playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, launchPower);
        RetroSfx.PlaySpring();
        JuiceManager.Dust(transform.position + Vector3.up * 0.2f, 8);
        JuiceManager.Shake(0.12f);
        StopAllCoroutines();
        StartCoroutine(SquashAnimation());
    }

    IEnumerator SquashAnimation()
    {
        float elapsed = 0f;
        const float duration = 0.28f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float squash = 1f + Mathf.Sin(progress * Mathf.PI) * 0.55f;
            transform.localScale = new Vector3(baseScale.x * squash, baseScale.y / squash, baseScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
    }

    static Sprite GetSpringSprite()
    {
        if (springSprite != null)
        {
            return springSprite;
        }

        const int width = 44;
        const int height = 30;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        Color pad = new Color(1f, 0.5f, 0.15f, 1f);
        Color coil = new Color(0.6f, 0.62f, 0.7f, 1f);
        Color arrow = Color.white;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = Color.clear;

                if (y >= height - 8)
                {
                    // Top pad with rounded corners
                    int edge = Mathf.Min(x, width - 1 - x);
                    if (edge >= 2 || y < height - 4)
                    {
                        color = pad;
                    }
                }
                else if (y >= 4)
                {
                    // Coil zig-zag
                    float wave = Mathf.PingPong(y * 2.4f, width * 0.5f) + width * 0.25f;
                    if (Mathf.Abs(x - wave) < 2.4f)
                    {
                        color = coil;
                    }
                }
                else if (x > width * 0.2f && x < width * 0.8f)
                {
                    color = coil;
                }

                // Up arrow on the pad
                if (y >= height - 7 && y <= height - 2)
                {
                    int rowFromTip = height - 2 - y;
                    if (Mathf.Abs(x - width * 0.5f) <= rowFromTip * 1.2f && rowFromTip <= 4)
                    {
                        color = arrow;
                    }
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        springSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 48f);
        return springSprite;
    }
}
