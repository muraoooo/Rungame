using UnityEngine;

// Plays after clearing the final stage: night sky fade-in, fireworks,
// scrolling credits, the player riding across the bottom and a gentle tune.
public class EndingDirector : MonoBehaviour
{
    static EndingDirector instance;

    public static bool IsActive => instance != null && instance.active;

    static Texture2D nightTexture;

    readonly string[] creditLines =
    {
        "YUNKEY RUN!",
        "",
        "☆ ぜんステージクリア!! ☆",
        "",
        "",
        "",
        "- スタッフ -",
        "",
        "しゅじんこう: YUNKEY",
        "ボス: キングスライム",
        "なかま: スライムたち",
        "",
        "せいさく: YUNKEY TEAM",
        "",
        "スペシャルサンクス:",
        "さいごまで はしってくれた きみ!",
        "",
        "",
        "THE END"
    };

    SpriteRenderer playerRenderer;
    bool active;
    bool statsAdded;
    float startTime;
    float nextFireworkTime;
    GUIStyle creditStyle;
    string statsLine;

    void Start()
    {
        instance = this;
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }

    void Update()
    {
        if (!active)
        {
            bool shouldStart = GameSession.HasEnded
                && GameSession.ReachedGoal
                && LevelManager.IsFinalStage;

            if (shouldStart)
            {
                BeginEnding();
            }
            return;
        }

        // Fireworks
        if (Time.unscaledTime >= nextFireworkTime && Time.unscaledTime - startTime > 1f)
        {
            nextFireworkTime = Time.unscaledTime + Random.Range(0.5f, 0.9f);
            LaunchFirework();
        }
    }

    void BeginEnding()
    {
        active = true;
        startTime = Time.unscaledTime;
        nextFireworkTime = startTime + 1.2f;

        int minutes = Mathf.FloorToInt(GameSession.ElapsedTime / 60f);
        float seconds = GameSession.ElapsedTime - minutes * 60f;
        statsLine = "スコア " + ScoreSystem.Score.ToString("N0")
            + "   タイム " + minutes.ToString("00") + ":" + seconds.ToString("00.00");

        EndBannerUI.Clear();
        RetroSfx.PlayEndingMusic();
    }

    void LaunchFirework()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null)
        {
            return;
        }

        Vector3 position = mainCamera.transform.position
            + new Vector3(Random.Range(-7f, 7f), Random.Range(0.5f, 4.5f), 10f);
        Color color = Color.HSVToRGB(Random.value, Random.Range(0.55f, 0.9f), 1f);
        JuiceManager.Firework(position, color);
        RetroSfx.PlayFirework();
    }

    void OnGUI()
    {
        if (!active)
        {
            return;
        }

        EnsureStyles();
        float width = Screen.width;
        float height = Screen.height;
        float scale = Mathf.Clamp(height / 1080f, 0.6f, 1.4f);
        float elapsed = Time.unscaledTime - startTime;

        DrawNightSky(width, height, elapsed);
        DrawStars(width, height, scale, elapsed);
        DrawCredits(width, height, scale, elapsed);
        DrawRidingPlayer(width, height, scale, elapsed);
        DrawPrompt(width, height, scale, elapsed);
    }

    void DrawNightSky(float width, float height, float elapsed)
    {
        if (nightTexture == null)
        {
            nightTexture = new Texture2D(1, 128, TextureFormat.RGBA32, false);
            nightTexture.filterMode = FilterMode.Bilinear;
            nightTexture.wrapMode = TextureWrapMode.Clamp;
            Color top = new Color(0.03f, 0.04f, 0.16f, 1f);
            Color bottom = new Color(0.16f, 0.08f, 0.3f, 1f);
            for (int y = 0; y < 128; y++)
            {
                nightTexture.SetPixel(0, y, Color.Lerp(bottom, top, y / 127f));
            }
            nightTexture.Apply();
        }

        float alpha = Mathf.Clamp01(elapsed / 1.4f) * 0.94f;
        Color original = GUI.color;
        GUI.color = new Color(1f, 1f, 1f, alpha);
        GUI.DrawTexture(new Rect(0f, 0f, width, height), nightTexture);
        GUI.color = original;
    }

    void DrawStars(float width, float height, float scale, float elapsed)
    {
        float skyAlpha = Mathf.Clamp01((elapsed - 0.8f) / 1f);
        if (skyAlpha <= 0f)
        {
            return;
        }

        Color original = GUI.color;
        for (int i = 0; i < 40; i++)
        {
            // Deterministic pseudo-random star positions
            float x = Mathf.Repeat(i * 0.61803f, 1f) * width;
            float y = Mathf.Repeat(i * 0.38196f + 0.07f, 1f) * height * 0.65f;
            float twinkle = 0.35f + Mathf.PingPong(elapsed * (0.8f + Mathf.Repeat(i * 0.327f, 1f)), 0.65f);
            float size = (2f + Mathf.Repeat(i * 0.731f, 1f) * 3f) * scale;

            GUI.color = new Color(1f, 1f, 0.92f, twinkle * skyAlpha);
            GUI.DrawTexture(new Rect(x, y, size, size), Texture2D.whiteTexture);
        }
        GUI.color = original;
    }

    void DrawCredits(float width, float height, float scale, float elapsed)
    {
        float lineHeight = 64f * scale;
        float scrollSpeed = 55f * scale;
        float totalHeight = (creditLines.Length + 4) * lineHeight;
        float scroll = Mathf.Min((elapsed - 1.5f) * scrollSpeed, totalHeight - height * 0.42f);

        if (elapsed < 1.5f)
        {
            return;
        }

        for (int i = 0; i < creditLines.Length; i++)
        {
            string line = creditLines[i];
            if (string.IsNullOrEmpty(line))
            {
                continue;
            }

            float y = height + i * lineHeight - scroll;
            if (y < -lineHeight || y > height + lineHeight)
            {
                continue;
            }

            bool isTitle = i == 0;
            bool isTheEnd = line == "THE END";
            creditStyle.fontSize = Mathf.RoundToInt((isTitle || isTheEnd ? 58f : 32f) * scale);

            Color color = Color.white;
            if (isTitle)
            {
                color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 0.15f, 1f), 0.5f, 1f);
            }
            else if (line.Contains("クリア"))
            {
                color = new Color(1f, 0.9f, 0.4f);
            }

            DrawOutlined(new Rect(0f, y, width, lineHeight), line, color, scale);
        }

        // Stats ride along right after THE END
        float statsY = height + (creditLines.Length + 1) * lineHeight - scroll;
        if (statsY < height + lineHeight)
        {
            creditStyle.fontSize = Mathf.RoundToInt(30f * scale);
            DrawOutlined(new Rect(0f, statsY, width, lineHeight), statsLine, new Color(0.75f, 0.95f, 1f), scale);
        }
    }

    void DrawRidingPlayer(float width, float height, float scale, float elapsed)
    {
        if (playerRenderer == null || playerRenderer.sprite == null)
        {
            return;
        }

        Sprite sprite = playerRenderer.sprite;
        float size = 150f * scale;
        float aspect = sprite.rect.width / sprite.rect.height;
        float x = Mathf.Repeat(elapsed * 85f, width + size * 2.5f) - size;
        float bob = Mathf.Sin(elapsed * 8f) * 4f * scale;

        Texture2D texture = sprite.texture;
        Rect uv = new Rect(
            sprite.rect.x / texture.width,
            sprite.rect.y / texture.height,
            sprite.rect.width / texture.width,
            sprite.rect.height / texture.height);
        GUI.DrawTextureWithTexCoords(new Rect(x, height * 0.84f + bob, size * aspect, size), texture, uv);
    }

    void DrawPrompt(float width, float height, float scale, float elapsed)
    {
        if (elapsed < 4f)
        {
            return;
        }

        float blink = 0.55f + Mathf.Sin(Time.unscaledTime * 4f) * 0.45f;
        creditStyle.fontSize = Mathf.RoundToInt(26f * scale);
        DrawOutlined(new Rect(0f, height - 48f * scale, width, 36f * scale),
            "ENTER で 1-1 から / R で 1-4 をもういちど", new Color(1f, 1f, 1f, blink), scale);
    }

    void EnsureStyles()
    {
        if (creditStyle != null)
        {
            return;
        }

        creditStyle = new GUIStyle(GUI.skin.label);
        creditStyle.alignment = TextAnchor.MiddleCenter;
        creditStyle.fontStyle = FontStyle.Bold;
    }

    void DrawOutlined(Rect rect, string text, Color color, float scale)
    {
        creditStyle.normal.textColor = new Color(0f, 0f, 0.05f, 0.85f * color.a);
        GUI.Label(new Rect(rect.x + 2f * scale, rect.y + 3f * scale, rect.width, rect.height), text, creditStyle);
        creditStyle.normal.textColor = color;
        GUI.Label(rect, text, creditStyle);
    }
}
