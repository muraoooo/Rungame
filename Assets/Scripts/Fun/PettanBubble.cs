using UnityEngine;

public class PettanBubble : MonoBehaviour
{
    const int MaxActiveBubbles = 2;

    static Sprite bubbleSprite;
    static int activeBubbleCount;

    float dieTime;
    bool popped;
    bool counted;

    public static PettanBubble Spawn(Vector3 position, Vector2 velocity)
    {
        if (activeBubbleCount >= MaxActiveBubbles)
        {
            return null;
        }

        GameObject bubbleObject = new GameObject("PettanBubble");
        bubbleObject.transform.position = position;

        SpriteRenderer renderer = bubbleObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetBubbleSprite();
        renderer.sortingOrder = 12;

        CircleCollider2D collider = bubbleObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.2f;

        Rigidbody2D body = bubbleObject.AddComponent<Rigidbody2D>();
        body.gravityScale = 0.55f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.linearVelocity = velocity;

        PettanBubble bubble = bubbleObject.AddComponent<PettanBubble>();
        bubble.dieTime = Time.time + 3.2f;
        bubble.counted = true;
        activeBubbleCount++;
        return bubble;
    }

    void OnDestroy()
    {
        if (counted)
        {
            activeBubbleCount = Mathf.Max(0, activeBubbleCount - 1);
            counted = false;
        }
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 10f) * 0.08f;
        transform.localScale = new Vector3(pulse, pulse, 1f);

        if (Time.time >= dieTime)
        {
            Pop();
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (popped)
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            RespawnSystem.KillPlayer(other.gameObject);
            Pop();
            return;
        }

        if (other.GetComponent<SlimeDamagingAttack>() != null)
        {
            Destroy(other.gameObject);
            Pop();
            return;
        }

        string objectName = other.gameObject.name;
        if (!other.isTrigger && (objectName.StartsWith("Ground") || objectName.StartsWith("Platform")))
        {
            Pop();
        }
    }

    void Pop()
    {
        if (popped)
        {
            return;
        }

        popped = true;
        JuiceManager.Burst(transform.position, new Color(0.55f, 0.9f, 1f), 6, 2.8f);
        Destroy(gameObject);
    }

    static Sprite GetBubbleSprite()
    {
        if (bubbleSprite != null)
        {
            return bubbleSprite;
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
                float radius = size * 0.42f;
                if (distance > radius)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float edge = Mathf.InverseLerp(radius, radius - 3f, distance);
                Color color = Color.Lerp(new Color(0.7f, 1f, 1f, 0.45f), new Color(0.85f, 1f, 1f, 0.85f), edge);
                if (x < size * 0.42f && y > size * 0.55f)
                {
                    color = Color.Lerp(color, Color.white, 0.55f);
                }
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        bubbleSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 72f);
        return bubbleSprite;
    }
}
