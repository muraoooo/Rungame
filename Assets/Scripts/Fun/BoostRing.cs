using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoostRing : MonoBehaviour
{
    const float ReuseCooldownSeconds = 3f;
    const string Stage5RingRootName = "__Stage5BoostRings";

    static Sprite ringSprite;
    static BoostRingBootstrap bootstrap;

    SpriteRenderer spriteRenderer;
    Vector3 baseScale;
    float readyTime;

    public static BoostRing Spawn(Vector3 position)
    {
        GameObject ringObject = new GameObject("BoostRing");
        ringObject.transform.position = position;

        SpriteRenderer renderer = ringObject.AddComponent<SpriteRenderer>();
        renderer.sprite = GetRingSprite();
        renderer.sortingOrder = 12;

        CircleCollider2D collider = ringObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 0.5f;

        return ringObject.AddComponent<BoostRing>();
    }

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void InstallStage5Spawner()
    {
        if (bootstrap != null)
        {
            return;
        }

        GameObject host = new GameObject("BoostRingBootstrap");
        Object.DontDestroyOnLoad(host);
        bootstrap = host.AddComponent<BoostRingBootstrap>();
    }

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        baseScale = transform.localScale;
    }

    void Update()
    {
        float pulse = 1f + Mathf.Sin(Time.time * 4f) * 0.1f;
        transform.localScale = baseScale * pulse;
        transform.Rotate(0f, 0f, 80f * Time.deltaTime);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Time.time < readyTime ? 0.38f : 1f;
            spriteRenderer.color = color;
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        TryBoost(other);
    }

    void OnTriggerStay2D(Collider2D other)
    {
        TryBoost(other);
    }

    void TryBoost(Collider2D other)
    {
        if (Time.time < readyTime || !other.CompareTag("Player"))
        {
            return;
        }

        Rigidbody2D playerBody = other.GetComponent<Rigidbody2D>();
        if (playerBody == null)
        {
            return;
        }

        float facing = PlayerFacing(other.gameObject);
        playerBody.linearVelocity = new Vector2(9f * facing, Mathf.Max(playerBody.linearVelocity.y, 4f));

        PlayerDash dash = other.GetComponent<PlayerDash>();
        if (dash != null)
        {
            dash.RechargeNow();
        }

        readyTime = Time.time + ReuseCooldownSeconds;
        ScoreSystem.AddTrick(transform.position);
        RetroSfx.PlayTrick();
        JuiceManager.Confetti(transform.position, 20);
        JuiceManager.Popup(transform.position + Vector3.up * 0.7f, "ビュン!", new Color(1f, 0.92f, 0.35f), 1.2f);
    }

    static float PlayerFacing(GameObject player)
    {
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            return playerRenderer.flipX ? -1f : 1f;
        }

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (body != null && Mathf.Abs(body.linearVelocity.x) > 0.05f)
        {
            return Mathf.Sign(body.linearVelocity.x);
        }

        return 1f;
    }

    static Sprite GetRingSprite()
    {
        if (ringSprite != null)
        {
            return ringSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        Color glow = new Color(1f, 0.74f, 0.12f, 0.45f);
        Color outer = new Color(1f, 0.64f, 0.05f, 1f);
        Color core = new Color(1f, 0.93f, 0.35f, 1f);
        Color shine = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 delta = new Vector2(x, y) - center;
                float radius = delta.magnitude;
                Color color = Color.clear;

                if (radius >= 18f && radius <= 26f)
                {
                    float t = Mathf.InverseLerp(26f, 18f, radius);
                    color = Color.Lerp(outer, core, Mathf.Sin(t * Mathf.PI));
                }
                else if (radius >= 14f && radius <= 30f)
                {
                    float alpha = Mathf.InverseLerp(30f, 26f, radius) * Mathf.InverseLerp(14f, 18f, radius);
                    color = new Color(glow.r, glow.g, glow.b, glow.a * Mathf.Clamp01(alpha));
                }

                if (radius >= 19f && radius <= 23f && delta.y > 5f && delta.x < -4f)
                {
                    color = Color.Lerp(color, shine, 0.55f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), 64f);
        return ringSprite;
    }

    class BoostRingBootstrap : MonoBehaviour
    {
        void OnEnable()
        {
            SceneManager.sceneLoaded += OnSceneLoaded;
            StartCoroutine(SpawnAfterLevelBuild());
        }

        void OnDisable()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
        }

        void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            StartCoroutine(SpawnAfterLevelBuild());
        }

        IEnumerator SpawnAfterLevelBuild()
        {
            yield return null;
            yield return null;

            if (!LevelManager.IsFinalStage || GameObject.Find(Stage5RingRootName) != null)
            {
                yield break;
            }

            GameObject root = new GameObject(Stage5RingRootName);
            BoostRing first = BoostRing.Spawn(new Vector3(27f, 4f, 0f));
            BoostRing second = BoostRing.Spawn(new Vector3(33f, 4.5f, 0f));
            first.transform.SetParent(root.transform);
            second.transform.SetParent(root.transform);
        }
    }
}
