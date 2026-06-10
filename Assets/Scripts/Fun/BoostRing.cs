using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class BoostRing : MonoBehaviour
{
    const float ReuseCooldownSeconds = 3f;
    const float BoostHorizontalSpeed = 9f;
    const float BoostVerticalFloor = 4f;
    const float BoostHoldSeconds = 0.28f;
    const string Stage5RingRootName = "__Stage5BoostRings";

    static Sprite ringSprite;
    static BoostRingBootstrap bootstrap;

    SpriteRenderer spriteRenderer;
    Vector3 baseScale;
    float spinOffset;
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
        spinOffset = Random.value * 10f;
    }

    void Update()
    {
        float time = Time.time + spinOffset;
        float pulse = 1f + Mathf.Sin(time * 3.2f) * 0.08f;
        transform.localScale = baseScale * pulse;
        transform.localRotation = Quaternion.Euler(0f, 0f, time * 36f);

        if (spriteRenderer != null)
        {
            Color color = spriteRenderer.color;
            color.a = Time.time < readyTime ? 0.35f : 1f;
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

        readyTime = Time.time + ReuseCooldownSeconds;

        float facing = PlayerFacing(other.gameObject, playerBody);
        playerBody.linearVelocity = new Vector2(
            BoostHorizontalSpeed * facing,
            Mathf.Max(playerBody.linearVelocity.y, BoostVerticalFloor));
        StartCoroutine(HoldBoostVelocity(playerBody, facing));

        PlayerDash dash = other.GetComponent<PlayerDash>();
        if (dash != null)
        {
            dash.RechargeNow();
        }

        ScoreSystem.AddTrick(transform.position);
        RetroSfx.PlayTrick();
        SpawnRainbowBurst(transform.position);
        JuiceManager.Popup(transform.position + Vector3.up * 0.7f, "ビュン!", new Color(0.95f, 1f, 0.45f), 1.2f);
    }

    IEnumerator HoldBoostVelocity(Rigidbody2D playerBody, float facing)
    {
        float endTime = Time.time + BoostHoldSeconds;
        WaitForFixedUpdate wait = new WaitForFixedUpdate();

        while (playerBody != null && Time.time < endTime)
        {
            yield return wait;
            playerBody.linearVelocity = new Vector2(
                BoostHorizontalSpeed * facing,
                Mathf.Max(playerBody.linearVelocity.y, BoostVerticalFloor));
        }
    }

    static float PlayerFacing(GameObject player, Rigidbody2D body)
    {
        SpriteRenderer playerRenderer = player.GetComponent<SpriteRenderer>();
        if (playerRenderer != null)
        {
            return playerRenderer.flipX ? -1f : 1f;
        }

        if (Mathf.Abs(body.linearVelocity.x) > 0.05f)
        {
            return Mathf.Sign(body.linearVelocity.x);
        }

        return player.transform.localScale.x < 0f ? -1f : 1f;
    }

    static void SpawnRainbowBurst(Vector3 position)
    {
        for (int i = 0; i < 6; i++)
        {
            Color color = Color.HSVToRGB(i / 6f, 0.85f, 1f);
            JuiceManager.Burst(position, color, 4, 5.8f);
        }
    }

    static Sprite GetRingSprite()
    {
        if (ringSprite != null)
        {
            return ringSprite;
        }

        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Point;
        Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);

        Color glow = new Color(1f, 0.78f, 0.12f, 0.42f);
        Color rim = new Color(1f, 0.92f, 0.28f, 1f);
        Color shade = new Color(0.95f, 0.46f, 0.05f, 1f);
        Color shine = Color.white;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float radius = Vector2.Distance(new Vector2(x, y), center);
                Color color = Color.clear;

                if (radius >= 18f && radius <= 25f)
                {
                    float highlight = Mathf.InverseLerp(25f, 18f, radius);
                    color = Color.Lerp(shade, rim, highlight);
                }
                else if ((radius >= 15f && radius < 18f) || (radius > 25f && radius <= 29f))
                {
                    float alpha = radius < 18f
                        ? Mathf.InverseLerp(15f, 18f, radius)
                        : Mathf.InverseLerp(29f, 25f, radius);
                    color = new Color(glow.r, glow.g, glow.b, glow.a * alpha);
                }

                bool sparklePixel = (x >= 46 && x <= 49 && y >= 45 && y <= 48)
                    || (x >= 14 && x <= 16 && y >= 17 && y <= 19)
                    || (x == 50 && y == 14)
                    || (x == 13 && y == 50);
                if (sparklePixel)
                {
                    color = Color.Lerp(color, shine, 0.8f);
                }

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        ringSprite = Sprite.Create(texture, new Rect(0f, 0f, size, size), new Vector2(0.5f, 0.5f), size);
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
