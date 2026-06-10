using UnityEngine;

// Cave-stage light effects: a shared radial glow sprite, the player's lantern,
// flickering torches and glowing magma pools. All fake-lit by drawing warm
// translucent glows above the darkness overlay (no URP light dependency).
public static class GlowFx
{
    static Sprite glowSprite;

    public static Sprite GetGlowSprite()
    {
        if (glowSprite != null)
        {
            return glowSprite;
        }

        const int size = 96;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center) / (size * 0.5f);
                float alpha = Mathf.Pow(Mathf.Clamp01(1f - distance), 2.2f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        glowSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 48f);
        return glowSprite;
    }

    public static SpriteRenderer CreateGlow(Transform parent, Vector3 localPosition, float scale, Color color, int sortingOrder)
    {
        GameObject glowObject = new GameObject("Glow");
        glowObject.transform.SetParent(parent, false);
        glowObject.transform.localPosition = localPosition;
        glowObject.transform.localScale = Vector3.one * scale;

        SpriteRenderer renderer = glowObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetGlowSprite();
        renderer.color = color;
        renderer.sortingOrder = sortingOrder;
        return renderer;
    }
}

// Warm lantern glow that follows the player through the dark.
public class PlayerLight : MonoBehaviour
{
    SpriteRenderer glow;
    float baseScale;

    void Start()
    {
        glow = GlowFx.CreateGlow(transform, Vector3.zero, 6.5f, new Color(1f, 0.88f, 0.62f, 0.5f), 6);
        baseScale = glow.transform.localScale.x;
    }

    void Update()
    {
        if (glow == null)
        {
            return;
        }

        // Subtle organic flicker
        float noise = Mathf.PerlinNoise(Time.time * 2.2f, 0.5f);
        float scale = baseScale * (0.96f + noise * 0.08f);
        glow.transform.localScale = new Vector3(scale, scale, 1f);
        Color color = glow.color;
        color.a = 0.44f + noise * 0.1f;
        glow.color = color;

        // FEVER turns the lantern rainbow
        if (ScoreSystem.IsFever)
        {
            Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.time * 1.2f, 1f), 0.45f, 1f);
            rainbow.a = color.a;
            glow.color = rainbow;
        }
    }
}

// Wall torch with flickering flame glow and rising embers.
public class Torch : MonoBehaviour
{
    static Sprite poleSprite;

    SpriteRenderer flameGlow;
    SpriteRenderer flameCore;
    float nextEmberTime;
    float flickerSeed;

    public static Torch Spawn(Vector2 groundPoint)
    {
        GameObject torchObject = new GameObject("Torch");
        torchObject.transform.position = new Vector3(groundPoint.x, groundPoint.y, 0f);

        SpriteRenderer renderer = torchObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetPoleSprite();
        renderer.sortingOrder = 6;

        return torchObject.AddComponent<Torch>();
    }

    void Start()
    {
        flickerSeed = Random.value * 10f;
        flameGlow = GlowFx.CreateGlow(transform, new Vector3(0f, 1.7f, 0f), 4.6f, new Color(1f, 0.62f, 0.2f, 0.5f), 6);
        flameCore = GlowFx.CreateGlow(transform, new Vector3(0f, 1.7f, 0f), 1.1f, new Color(1f, 0.92f, 0.5f, 0.95f), 7);
    }

    void Update()
    {
        float noise = Mathf.PerlinNoise(Time.time * 6f, flickerSeed);

        if (flameGlow != null)
        {
            float scale = 4.2f + noise * 1f;
            flameGlow.transform.localScale = new Vector3(scale, scale, 1f);
            Color color = flameGlow.color;
            color.a = 0.4f + noise * 0.2f;
            flameGlow.color = color;
        }

        if (flameCore != null)
        {
            float scale = 1f + noise * 0.35f;
            flameCore.transform.localScale = new Vector3(scale, scale * (1.15f + noise * 0.2f), 1f);
        }

        if (Time.time >= nextEmberTime)
        {
            nextEmberTime = Time.time + Random.Range(0.25f, 0.6f);
            JuiceManager.Embers(transform.position + new Vector3(Random.Range(-0.15f, 0.15f), 1.7f, 0f),
                new Color(1f, 0.7f, 0.25f, 0.9f), 1);
        }
    }

    static Sprite GetPoleSprite()
    {
        if (poleSprite != null)
        {
            return poleSprite;
        }

        const int width = 12;
        const int height = 64;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Color wood = new Color(0.42f, 0.3f, 0.2f, 1f);
        Color band = new Color(0.6f, 0.55f, 0.5f, 1f);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                Color color = Color.clear;
                if (x >= 4 && x <= 7)
                {
                    color = y > height - 14 ? band : wood;
                }
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        poleSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 38f);
        return poleSprite;
    }
}

// Glowing, pulsing magma pool. Touching it costs a life (checkpoint respawn).
public class MagmaPool : MonoBehaviour
{
    static Sprite magmaSprite;

    SpriteRenderer surface;
    SpriteRenderer glow;
    float nextEmberTime;
    float pulseSeed;

    public static MagmaPool Spawn(Vector2 groundPoint, float width)
    {
        GameObject magmaObject = new GameObject("MagmaPool");
        magmaObject.transform.position = new Vector3(groundPoint.x, groundPoint.y, 0f);
        magmaObject.transform.localScale = new Vector3(width, 1f, 1f);

        SpriteRenderer renderer = magmaObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetMagmaSprite();
        renderer.sortingOrder = 6;

        BoxCollider2D collider = magmaObject.AddComponent<BoxCollider2D>();
        collider.isTrigger = true;
        collider.size = new Vector2(0.88f, 0.2f);
        collider.offset = new Vector2(0f, 0.1f);

        return magmaObject.AddComponent<MagmaPool>();
    }

    void Start()
    {
        pulseSeed = Random.value * 10f;
        surface = GetComponent<SpriteRenderer>();
        glow = GlowFx.CreateGlow(transform, new Vector3(0f, 0.25f, 0f), 3.4f, new Color(1f, 0.42f, 0.12f, 0.5f), 6);

        // Glow should stay round even though the pool is stretched horizontally
        if (glow != null)
        {
            float inverseX = transform.localScale.x > 0.01f ? 1f / transform.localScale.x : 1f;
            glow.transform.localScale = new Vector3(3.4f * inverseX * 1.6f, 3.4f, 1f);
        }
    }

    void Update()
    {
        float pulse = Mathf.PerlinNoise(Time.time * 1.8f, pulseSeed);

        if (surface != null)
        {
            surface.color = Color.Lerp(new Color(1f, 0.55f, 0.2f, 1f), new Color(1f, 0.85f, 0.35f, 1f), pulse);
        }

        if (glow != null)
        {
            Color color = glow.color;
            color.a = 0.38f + pulse * 0.22f;
            glow.color = color;
        }

        if (Time.time >= nextEmberTime)
        {
            nextEmberTime = Time.time + Random.Range(0.2f, 0.5f);
            float spread = transform.localScale.x * 0.4f;
            JuiceManager.Embers(transform.position + new Vector3(Random.Range(-spread, spread), 0.2f, 0f),
                new Color(1f, 0.5f, 0.15f, 0.9f), 1);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Burn(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        Burn(other);
    }

    void Burn(Collider2D other)
    {
        if (GameSession.HasEnded || !other.CompareTag("Player"))
        {
            return;
        }

        RespawnSystem.KillPlayer(other.gameObject);
    }

    static Sprite GetMagmaSprite()
    {
        if (magmaSprite != null)
        {
            return magmaSprite;
        }

        const int width = 64;
        const int height = 18;
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                // Wavy surface line with bright veins
                float surfaceLine = height - 4f + Mathf.Sin(x * 0.55f) * 2f;
                if (y > surfaceLine)
                {
                    texture.SetPixel(x, y, Color.clear);
                    continue;
                }

                float vein = Mathf.PerlinNoise(x * 0.3f, y * 0.45f);
                Color color = vein > 0.62f
                    ? new Color(1f, 0.9f, 0.45f, 1f)
                    : Color.Lerp(new Color(0.75f, 0.18f, 0.05f, 1f), new Color(1f, 0.45f, 0.1f, 1f), vein);
                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        magmaSprite = Sprite.Create(texture, new Rect(0f, 0f, width, height), new Vector2(0.5f, 0f), 64f);
        return magmaSprite;
    }
}
