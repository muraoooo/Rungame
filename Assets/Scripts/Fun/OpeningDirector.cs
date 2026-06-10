using UnityEngine;

// Fully code-drawn title screen: gradient sky, drifting clouds, a bouncing
// rainbow logo, the player riding across the bottom, spinning coins,
// best-time list and a looping title tune.
public class OpeningDirector : MonoBehaviour
{
    const string LogoText = "YUNKEY RUN!";

    static Texture2D gradientTexture;
    static Texture2D circleTexture;

    SpriteRenderer playerRenderer;
    Sprite cloudA;
    Sprite cloudB;
    Sprite coinSprite;
    GUIStyle logoStyle;
    GUIStyle textStyle;
    bool bannerCleared;
    bool musicStarted;

    void Start()
    {
        cloudA = Resources.Load<Sprite>("Clouds/CloudA");
        cloudB = Resources.Load<Sprite>("Clouds/CloudB");

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            playerRenderer = player.GetComponent<SpriteRenderer>();
        }
    }

    void Update()
    {
        if (GameSession.HasStarted)
        {
            if (musicStarted)
            {
                musicStarted = false;
                RetroSfx.StopMusic();
            }
            return;
        }

        // The scene's StartScreen shows its banner in Start; our opening
        // replaces it, so clear it once all Starts have run.
        if (!bannerCleared)
        {
            bannerCleared = true;
            EndBannerUI.Clear();
        }

        if (!musicStarted)
        {
            musicStarted = true;
            RetroSfx.PlayTitleMusic();
        }
    }

    void OnGUI()
    {
        if (GameSession.HasStarted)
        {
            return;
        }

        EnsureStyles();
        float width = Screen.width;
        float height = Screen.height;
        float scale = Mathf.Clamp(height / 1080f, 0.6f, 1.4f);
        float time = Time.unscaledTime;

        DrawSky(width, height);
        DrawSun(width, height, scale, time);
        DrawClouds(width, height, scale, time);
        DrawLogo(width, height, scale, time);
        DrawCoinRow(width, height, scale, time);
        DrawRidingPlayer(width, height, scale, time);
        DrawPrompts(width, height, scale, time);
        DrawBestTimes(width, scale);
    }

    void DrawSky(float width, float height)
    {
        if (gradientTexture == null)
        {
            gradientTexture = new Texture2D(1, 128, TextureFormat.RGBA32, false);
            gradientTexture.filterMode = FilterMode.Bilinear;
            gradientTexture.wrapMode = TextureWrapMode.Clamp;
            Color top = new Color(0.3f, 0.62f, 0.95f, 1f);
            Color bottom = new Color(0.78f, 0.93f, 1f, 1f);
            for (int y = 0; y < 128; y++)
            {
                gradientTexture.SetPixel(0, y, Color.Lerp(bottom, top, y / 127f));
            }
            gradientTexture.Apply();
        }

        GUI.DrawTexture(new Rect(0f, 0f, width, height), gradientTexture);
    }

    void DrawSun(float width, float height, float scale, float time)
    {
        if (circleTexture == null)
        {
            const int size = 64;
            circleTexture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            circleTexture.filterMode = FilterMode.Bilinear;
            Vector2 center = new Vector2((size - 1) * 0.5f, (size - 1) * 0.5f);
            for (int y = 0; y < size; y++)
            {
                for (int x = 0; x < size; x++)
                {
                    float distance = Vector2.Distance(new Vector2(x, y), center);
                    float alpha = Mathf.InverseLerp(size * 0.5f, size * 0.35f, distance);
                    circleTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
                }
            }
            circleTexture.Apply();
        }

        float pulse = 1f + Mathf.Sin(time * 1.4f) * 0.04f;
        float sunSize = 170f * scale * pulse;
        DrawTinted(new Rect(width - sunSize * 1.35f, sunSize * 0.18f, sunSize, sunSize),
            circleTexture, new Color(1f, 0.92f, 0.55f, 0.95f));
    }

    void DrawClouds(float width, float height, float scale, float time)
    {
        DrawCloud(cloudA, Mathf.Repeat(time * 28f, width + 420f) - 420f, height * 0.16f, 300f * scale, 0.9f);
        DrawCloud(cloudB, Mathf.Repeat(time * 18f + width * 0.5f, width + 360f) - 360f, height * 0.32f, 230f * scale, 0.75f);
        DrawCloud(cloudA, Mathf.Repeat(time * 40f + width * 0.25f, width + 500f) - 500f, height * 0.06f, 360f * scale, 0.65f);
    }

    void DrawCloud(Sprite sprite, float x, float y, float size, float alpha)
    {
        if (sprite == null)
        {
            return;
        }

        float aspect = sprite.rect.width / sprite.rect.height;
        DrawSprite(new Rect(x, y, size, size / aspect), sprite, new Color(1f, 1f, 1f, alpha));
    }

    void DrawLogo(float width, float height, float scale, float time)
    {
        float letterWidth = 74f * scale;
        float totalWidth = LogoText.Length * letterWidth;
        float startX = (width - totalWidth) * 0.5f;
        float baseY = height * 0.2f;

        logoStyle.fontSize = Mathf.RoundToInt(96f * scale);

        for (int i = 0; i < LogoText.Length; i++)
        {
            string letter = LogoText[i].ToString();
            if (letter == " ")
            {
                continue;
            }

            float bounce = Mathf.Sin(time * 3.2f + i * 0.55f) * 14f * scale;
            Color color = Color.HSVToRGB(Mathf.Repeat(i * 0.09f + time * 0.12f, 1f), 0.65f, 1f);
            Rect rect = new Rect(startX + i * letterWidth, baseY + bounce, letterWidth * 1.6f, 120f * scale);

            logoStyle.normal.textColor = new Color(0.1f, 0.15f, 0.3f, 0.85f);
            GUI.Label(new Rect(rect.x + 4f * scale, rect.y + 5f * scale, rect.width, rect.height), letter, logoStyle);
            logoStyle.normal.textColor = color;
            GUI.Label(rect, letter, logoStyle);
        }
    }

    void DrawCoinRow(float width, float height, float scale, float time)
    {
        Sprite coin = GetCoinSprite();
        if (coin == null)
        {
            return;
        }

        float size = 44f * scale;
        for (int i = 0; i < 5; i++)
        {
            float spin = Mathf.Abs(Mathf.Cos(time * 4f + i * 0.7f));
            float x = width * 0.5f + (i - 2) * size * 1.6f;
            float y = height * 0.42f + Mathf.Sin(time * 2.5f + i * 0.8f) * 6f * scale;
            DrawSprite(new Rect(x - size * spin * 0.5f, y, size * spin, size), coin, Color.white);
        }
    }

    Sprite GetCoinSprite()
    {
        if (coinSprite == null)
        {
            Coin sample = FindAnyObjectByType<Coin>();
            if (sample != null)
            {
                SpriteRenderer renderer = sample.GetComponent<SpriteRenderer>();
                coinSprite = renderer != null ? renderer.sprite : null;
            }
        }

        return coinSprite;
    }

    void DrawRidingPlayer(float width, float height, float scale, float time)
    {
        if (playerRenderer == null || playerRenderer.sprite == null)
        {
            return;
        }

        Sprite sprite = playerRenderer.sprite;
        float size = 200f * scale;
        float aspect = sprite.rect.width / sprite.rect.height;
        float x = Mathf.Repeat(time * 130f, width + size * 2f) - size;
        float bob = Mathf.Sin(time * 9f) * 5f * scale;
        DrawSprite(new Rect(x, height * 0.78f + bob, size * aspect, size), sprite, Color.white);
    }

    void DrawPrompts(float width, float height, float scale, float time)
    {
        float blink = 0.62f + Mathf.Sin(time * 5f) * 0.38f;
        textStyle.fontSize = Mathf.RoundToInt(42f * scale);
        DrawOutlined(new Rect(0f, height * 0.55f, width, 56f * scale),
            "SPACE でスタート!", new Color(1f, 0.95f, 0.5f, blink), scale);

        textStyle.fontSize = Mathf.RoundToInt(26f * scale);
        DrawOutlined(new Rect(0f, height * 0.55f + 64f * scale, width, 36f * scale),
            "ステージ 1-" + LevelManager.CurrentStage + " (1〜4キーで えらべる)", Color.white, scale);

        textStyle.fontSize = Mathf.RoundToInt(19f * scale);
        DrawOutlined(new Rect(0f, height - 46f * scale, width, 30f * scale),
            "←→:はしる SPACE:ジャンプ Q:アクロバット F:こうげき SHIFT:ダッシュ W:ひっさつ R:やりなおし", Color.white, scale);
    }

    void DrawBestTimes(float width, float scale)
    {
        textStyle.fontSize = Mathf.RoundToInt(22f * scale);
        float y = 24f * scale;
        bool anyRecord = false;

        for (int stage = 1; stage <= LevelManager.MaxStage; stage++)
        {
            float best = PlayerPrefs.GetFloat("RungameBestTime_S" + stage, 0f);
            if (best <= 0f)
            {
                continue;
            }

            if (!anyRecord)
            {
                anyRecord = true;
                DrawOutlinedAt(new Rect(width - 320f * scale, y, 300f * scale, 30f * scale),
                    "- BEST TIMES -", new Color(1f, 0.9f, 0.5f), scale, TextAnchor.MiddleRight);
                y += 32f * scale;
            }

            int minutes = Mathf.FloorToInt(best / 60f);
            string text = "1-" + stage + "  " + minutes.ToString("00") + ":" + (best - minutes * 60f).ToString("00.00");
            DrawOutlinedAt(new Rect(width - 320f * scale, y, 300f * scale, 30f * scale),
                text, Color.white, scale, TextAnchor.MiddleRight);
            y += 30f * scale;
        }
    }

    void EnsureStyles()
    {
        if (logoStyle != null)
        {
            return;
        }

        logoStyle = new GUIStyle(GUI.skin.label);
        logoStyle.alignment = TextAnchor.MiddleCenter;
        logoStyle.fontStyle = FontStyle.Bold;

        textStyle = new GUIStyle(GUI.skin.label);
        textStyle.alignment = TextAnchor.MiddleCenter;
        textStyle.fontStyle = FontStyle.Bold;
    }

    void DrawOutlined(Rect rect, string text, Color color, float scale)
    {
        DrawOutlinedAt(rect, text, color, scale, TextAnchor.MiddleCenter);
    }

    void DrawOutlinedAt(Rect rect, string text, Color color, float scale, TextAnchor anchor)
    {
        TextAnchor original = textStyle.alignment;
        textStyle.alignment = anchor;

        textStyle.normal.textColor = new Color(0.08f, 0.12f, 0.25f, 0.8f * color.a);
        GUI.Label(new Rect(rect.x + 2f * scale, rect.y + 3f * scale, rect.width, rect.height), text, textStyle);
        textStyle.normal.textColor = color;
        GUI.Label(rect, text, textStyle);

        textStyle.alignment = original;
    }

    static void DrawSprite(Rect rect, Sprite sprite, Color color)
    {
        Texture2D texture = sprite.texture;
        Rect uv = new Rect(
            sprite.rect.x / texture.width,
            sprite.rect.y / texture.height,
            sprite.rect.width / texture.width,
            sprite.rect.height / texture.height);

        Color original = GUI.color;
        GUI.color = color;
        GUI.DrawTextureWithTexCoords(rect, texture, uv);
        GUI.color = original;
    }

    static void DrawTinted(Rect rect, Texture2D texture, Color color)
    {
        Color original = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, texture);
        GUI.color = original;
    }
}
