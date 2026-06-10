using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[DefaultExecutionOrder(1000)]
public class JuiceManager : MonoBehaviour
{
    public static JuiceManager Instance { get; private set; }

    class FloatingPopup
    {
        public string text;
        public Vector3 worldPosition;
        public float startTime;
        public Color color;
        public float size;
    }

    const float PopupLifeSeconds = 1.05f;

    readonly List<FloatingPopup> popups = new List<FloatingPopup>();
    float trauma;
    Sprite particleSprite;
    GUIStyle popupStyle;
    Coroutine hitStopCoroutine;

    void Awake()
    {
        Instance = this;
        particleSprite = CreateSoftCircleSprite();
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }

    void LateUpdate()
    {
        if (trauma <= 0f)
        {
            return;
        }

        trauma = Mathf.Max(0f, trauma - Time.unscaledDeltaTime * 2.4f);

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        float shakeStrength = trauma * trauma;
        Vector3 shakeOffset = new Vector3(
            Mathf.PerlinNoise(Time.unscaledTime * 23f, 0.3f) - 0.5f,
            Mathf.PerlinNoise(0.7f, Time.unscaledTime * 23f) - 0.5f,
            0f) * shakeStrength * 0.9f;
        mainCamera.transform.position += shakeOffset;
    }

    public static void Shake(float amount)
    {
        if (Instance != null)
        {
            Instance.trauma = Mathf.Clamp01(Instance.trauma + amount);
        }
    }

    public static void HitStop(float seconds)
    {
        if (Instance == null || !GameSession.HasStarted || GameSession.HasEnded)
        {
            return;
        }

        if (Instance.hitStopCoroutine != null)
        {
            Instance.StopCoroutine(Instance.hitStopCoroutine);
        }

        Instance.hitStopCoroutine = Instance.StartCoroutine(Instance.HitStopRoutine(seconds));
    }

    IEnumerator HitStopRoutine(float seconds)
    {
        Time.timeScale = 0.05f;
        yield return new WaitForSecondsRealtime(seconds);

        if (GameSession.HasStarted && !GameSession.HasEnded)
        {
            Time.timeScale = 1f;
        }

        hitStopCoroutine = null;
    }

    public static void Popup(Vector3 worldPosition, string text, Color color, float size)
    {
        if (Instance == null)
        {
            return;
        }

        Instance.popups.Add(new FloatingPopup
        {
            text = text,
            worldPosition = worldPosition,
            startTime = Time.unscaledTime,
            color = color,
            size = size
        });
    }

    public static void Burst(Vector3 position, Color color, int count, float speed)
    {
        if (Instance == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float magnitude = speed * Random.Range(0.4f, 1.1f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Abs(Mathf.Sin(angle)) * 0.9f + 0.25f) * magnitude;
            Instance.SpawnParticle(position, color, velocity, 14f, Random.Range(0.35f, 0.7f), Random.Range(0.65f, 1.3f));
        }
    }

    public static void Confetti(Vector3 position, int count)
    {
        if (Instance == null)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            Color color = Color.HSVToRGB(Random.value, Random.Range(0.6f, 0.95f), 1f);
            Vector2 velocity = new Vector2(Random.Range(-5.5f, 5.5f), Random.Range(4f, 11f));
            Instance.SpawnParticle(position, color, velocity, 8f, Random.Range(1.1f, 2.1f), Random.Range(0.8f, 1.6f));
        }
    }

    public static void Dust(Vector3 position, int count)
    {
        if (Instance == null)
        {
            return;
        }

        Color dustColor = new Color(0.93f, 0.9f, 0.8f, 0.85f);
        for (int i = 0; i < count; i++)
        {
            Vector2 velocity = new Vector2(Random.Range(-2.6f, 2.6f), Random.Range(0.6f, 2.4f));
            Instance.SpawnParticle(position, dustColor, velocity, 4.5f, Random.Range(0.25f, 0.5f), Random.Range(0.7f, 1.2f));
        }
    }

    public static void SpawnGhost(SpriteRenderer source, Transform sourceTransform, bool rainbow)
    {
        if (Instance == null || source == null || source.sprite == null)
        {
            return;
        }

        GameObject ghost = new GameObject("DashGhost");
        ghost.transform.position = sourceTransform.position;
        ghost.transform.localScale = sourceTransform.lossyScale;

        SpriteRenderer ghostRenderer = ghost.AddComponent<SpriteRenderer>();
        ghostRenderer.sprite = source.sprite;
        ghostRenderer.flipX = source.flipX;
        ghostRenderer.sortingOrder = source.sortingOrder - 1;
        ghostRenderer.color = rainbow
            ? WithAlpha(Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.4f, 1f), 0.75f, 1f), 0.55f)
            : new Color(0.55f, 0.8f, 1f, 0.45f);

        FunParticle particle = ghost.AddComponent<FunParticle>();
        particle.velocity = Vector2.zero;
        particle.gravity = 0f;
        particle.life = 0.32f;
        particle.shrink = 0.25f;
    }

    static Color WithAlpha(Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    void SpawnParticle(Vector3 position, Color color, Vector2 velocity, float gravity, float life, float scale)
    {
        GameObject particleObject = new GameObject("FunParticle");
        particleObject.transform.position = position;
        particleObject.transform.localScale = Vector3.one * scale * 0.4f;

        SpriteRenderer renderer = particleObject.AddComponent<SpriteRenderer>();
        renderer.sprite = particleSprite;
        renderer.color = color;
        renderer.sortingOrder = 30;

        FunParticle particle = particleObject.AddComponent<FunParticle>();
        particle.velocity = velocity;
        particle.gravity = gravity;
        particle.life = life;
    }

    Sprite CreateSoftCircleSprite()
    {
        const int size = 24;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                float alpha = Mathf.InverseLerp(size * 0.5f, size * 0.28f, distance);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        texture.Apply();
        return Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
    }

    void OnGUI()
    {
        if (popups.Count == 0)
        {
            return;
        }

        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        if (popupStyle == null)
        {
            popupStyle = new GUIStyle(GUI.skin.label);
            popupStyle.alignment = TextAnchor.MiddleCenter;
            popupStyle.fontStyle = FontStyle.Bold;
        }

        float uiScale = Mathf.Clamp(Screen.height / 1080f, 0.74f, 1.2f);

        for (int i = popups.Count - 1; i >= 0; i--)
        {
            FloatingPopup popup = popups[i];
            float age = Time.unscaledTime - popup.startTime;
            if (age >= PopupLifeSeconds)
            {
                popups.RemoveAt(i);
                continue;
            }

            float progress = age / PopupLifeSeconds;
            Vector3 worldPosition = popup.worldPosition + Vector3.up * (progress * 1.3f);
            Vector3 screenPoint = mainCamera.WorldToScreenPoint(worldPosition);
            if (screenPoint.z < 0f)
            {
                continue;
            }

            float alpha = progress < 0.65f ? 1f : 1f - (progress - 0.65f) / 0.35f;
            float popScale = 1f + Mathf.Max(0f, 0.35f - age * 2.2f);
            popupStyle.fontSize = Mathf.RoundToInt(34f * popup.size * uiScale * popScale);

            Rect rect = new Rect(screenPoint.x - 150f, Screen.height - screenPoint.y - 30f, 300f, 60f);

            popupStyle.normal.textColor = new Color(0.05f, 0.08f, 0.12f, 0.75f * alpha);
            GUI.Label(new Rect(rect.x + 2f, rect.y + 3f, rect.width, rect.height), popup.text, popupStyle);

            popupStyle.normal.textColor = WithAlpha(popup.color, alpha);
            GUI.Label(rect, popup.text, popupStyle);
        }
    }
}

public class FunParticle : MonoBehaviour
{
    public Vector2 velocity;
    public float gravity = 14f;
    public float life = 0.6f;
    public float shrink = 0.85f;

    float age;
    SpriteRenderer spriteRenderer;
    Vector3 startScale;
    Color startColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        startScale = transform.localScale;
        if (spriteRenderer != null)
        {
            startColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        float deltaTime = Time.unscaledDeltaTime;
        age += deltaTime;

        if (age >= life)
        {
            Destroy(gameObject);
            return;
        }

        velocity.y -= gravity * deltaTime;
        transform.position += (Vector3)(velocity * deltaTime);

        float progress = age / life;
        transform.localScale = startScale * (1f - progress * shrink);

        if (spriteRenderer != null)
        {
            Color color = startColor;
            color.a = startColor.a * (1f - progress * progress);
            spriteRenderer.color = color;
        }
    }
}
