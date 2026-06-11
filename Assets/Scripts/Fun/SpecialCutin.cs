using UnityEngine;

// Full-screen manga-style cut-in for the W special attack.
// If a sprite exists at Resources/UI/SpecialCutin it is treated as finished
// cut-in artwork; otherwise the player sprite is blown up over speed lines.
public class SpecialCutin : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    const float Duration = 1.18f;
    const float BlastRadius = 16f;

    static Texture2D speedLineTexture;

    GameObject player;
    Sprite cutinSprite;
    Sprite playerSprite;
    float startTime;
    bool unleashed;
    GUIStyle titleStyle;
    GUIStyle nameStyle;
    GUIStyle commentStyle;

    public static void Play(GameObject player)
    {
        if (IsPlaying || player == null)
        {
            return;
        }

        GameObject host = new GameObject("SpecialCutin");
        SpecialCutin cutin = host.AddComponent<SpecialCutin>();
        cutin.player = player;
    }

    void Start()
    {
        IsPlaying = true;
        startTime = Time.unscaledTime;
        cutinSprite = Resources.Load<Sprite>("UI/SpecialCutin");

        SpriteRenderer renderer = player != null ? player.GetComponent<SpriteRenderer>() : null;
        playerSprite = renderer != null ? renderer.sprite : null;

        Time.timeScale = 0f;
        RetroSfx.PlayCutin();
        JuiceManager.Shake(0.25f);
    }

    void Update()
    {
        if (Time.unscaledTime - startTime < Duration || unleashed)
        {
            return;
        }

        unleashed = true;
        Unleash();
        Destroy(gameObject);
    }

    void OnDestroy()
    {
        IsPlaying = false;
        if (GameSession.HasStarted && !GameSession.HasEnded)
        {
            Time.timeScale = 1f;
        }
    }

    void Unleash()
    {
        if (GameSession.HasStarted && !GameSession.HasEnded)
        {
            Time.timeScale = 1f;
        }

        RetroSfx.PlaySpecialBoom();
        JuiceManager.Shake(0.6f);

        if (player == null)
        {
            return;
        }

        Vector3 origin = player.transform.position;
        JuiceManager.Burst(origin, new Color(0.55f, 1f, 0.4f), 26, 9f);
        JuiceManager.Popup(origin + Vector3.up * 2f, "ドッカーン!!", new Color(0.6f, 1f, 0.45f), 1.5f);

        GameOver[] enemies = Object.FindObjectsByType<GameOver>(FindObjectsSortMode.None);
        foreach (GameOver enemy in enemies)
        {
            if (Vector3.Distance(enemy.transform.position, origin) > BlastRadius)
            {
                continue;
            }

            JuiceManager.Burst(enemy.transform.position, new Color(0.5f, 0.9f, 0.45f), 12, 6f);
            enemy.DefeatBySpecial(origin);
        }

        VariantEnemy[] variantEnemies = Object.FindObjectsByType<VariantEnemy>(FindObjectsSortMode.None);
        foreach (VariantEnemy enemy in variantEnemies)
        {
            if (Vector3.Distance(enemy.transform.position, origin) > BlastRadius)
            {
                continue;
            }

            JuiceManager.Burst(enemy.transform.position, new Color(0.5f, 0.9f, 0.65f), 12, 6f);
            enemy.DefeatBySpecial(origin);
        }
    }

    void OnGUI()
    {
        float progress = Mathf.Clamp01((Time.unscaledTime - startTime) / Duration);
        float width = Screen.width;
        float height = Screen.height;
        float scale = Mathf.Clamp(height / 1080f, 0.6f, 1.4f);

        EnsureStyles();

        if (cutinSprite != null)
        {
            DrawFinishedCutin(progress, width, height, scale);
        }
        else
        {
            DrawFallbackCutin(progress, width, height, scale);
        }

        // White flash at start and end
        float flash = Mathf.Max(
            1f - progress / 0.08f,
            (progress - 0.93f) / 0.07f);
        if (flash > 0f)
        {
            DrawTinted(new Rect(0f, 0f, width, height), Texture2D.whiteTexture, new Color(1f, 1f, 1f, Mathf.Clamp01(flash)));
        }
    }

    void DrawFinishedCutin(float progress, float width, float height, float scale)
    {
        DrawTinted(new Rect(0f, 0f, width, height), Texture2D.whiteTexture, Color.black);

        float slam = 1f - Mathf.Pow(1f - Mathf.Clamp01(progress / 0.18f), 4f);
        float settle = Mathf.Clamp01((progress - 0.18f) / 0.7f);
        float shake = (1f - progress) * 14f * scale;
        float xKick = Mathf.Lerp(-width * 0.55f, 0f, slam) + Mathf.Sin(Time.unscaledTime * 84f) * shake;
        float yKick = Mathf.Cos(Time.unscaledTime * 71f) * shake * 0.35f;
        float zoom = Mathf.Lerp(1.18f, 1.03f, settle) + Mathf.Sin(Time.unscaledTime * 18f) * 0.01f;

        Rect artRect = CoverRect(cutinSprite, width, height, zoom);
        artRect.x += xKick;
        artRect.y += yKick;
        DrawSprite(artRect, cutinSprite, Color.white);

        Texture2D lines = GetSpeedLineTexture();
        float lineAlpha = Mathf.Lerp(0.7f, 0.22f, progress);
        DrawRotated(new Rect(-width * 0.25f, -height * 0.25f, width * 1.5f, height * 1.5f),
            lines, new Color(0.55f, 1f, 0.25f, lineAlpha), -Time.unscaledTime * 24f);

        float barHeight = 58f * scale;
        DrawTinted(new Rect(0f, 0f, width, barHeight), Texture2D.whiteTexture, new Color(0f, 0f, 0f, 0.55f));
        DrawTinted(new Rect(0f, height - barHeight, width, barHeight), Texture2D.whiteTexture, new Color(0f, 0f, 0f, 0.58f));

        DrawSlashBand(width, height, progress, scale);
        DrawSpecialText(width, height, progress, scale);
    }

    void DrawFallbackCutin(float progress, float width, float height, float scale)
    {
        // Dark background
        DrawTinted(new Rect(0f, 0f, width, height), Texture2D.whiteTexture, new Color(0.02f, 0.04f, 0.03f, 0.9f));

        // Rotating speed lines (white + green layers)
        Texture2D lines = GetSpeedLineTexture();
        DrawRotated(new Rect(-width * 0.25f, -height * 0.25f, width * 1.5f, height * 1.5f),
            lines, new Color(1f, 1f, 1f, 0.85f), Time.unscaledTime * 25f);
        DrawRotated(new Rect(-width * 0.25f, -height * 0.25f, width * 1.5f, height * 1.5f),
            lines, new Color(0.45f, 1f, 0.35f, 0.5f), -Time.unscaledTime * 18f);

        // Artwork slides in from the left
        float slide = 1f - Mathf.Pow(1f - Mathf.Clamp01(progress / 0.22f), 3f);
        float drift = (progress - 0.22f) * 30f * scale;
        float artHeight = height * 0.62f;

        if (playerSprite != null)
        {
            float aspect = playerSprite.rect.width / playerSprite.rect.height;
            float artWidth = artHeight * aspect;
            float artX = Mathf.Lerp(-artWidth, (width - artWidth) * 0.5f, slide) + Mathf.Max(0f, drift);
            DrawSprite(new Rect(artX, (height - artHeight) * 0.45f, artWidth, artHeight), playerSprite, Color.white);
        }

        // Text: "ひっさつ!!" top, technique name bottom
        float textSlide = 1f - Mathf.Pow(1f - Mathf.Clamp01((progress - 0.1f) / 0.2f), 3f);
        if (textSlide > 0f)
        {
            titleStyle.fontSize = Mathf.RoundToInt(64f * scale);
            float titleX = Mathf.Lerp(width, width * 0.5f - 300f * scale, textSlide);
            DrawOutlinedRotated(new Rect(titleX, height * 0.1f, 600f * scale, 90f * scale),
                "ひっさつ!!", titleStyle, new Color(1f, 0.95f, 0.4f), -5f);

            float pulse = 1f + Mathf.Sin(Time.unscaledTime * 16f) * 0.06f;
            nameStyle.fontSize = Mathf.RoundToInt(86f * scale * pulse);
            Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.8f, 1f), 0.6f, 1f);
            DrawOutlinedRotated(new Rect(0f, height * 0.72f, width, 120f * scale),
                "メガスマッシュ!!", nameStyle, rainbow, -3f);

            DrawCommentBubble(new Rect(width * 0.62f, height * 0.2f, 260f * scale, 74f * scale),
                "いくよ!", scale, new Color(1f, 1f, 1f, 0.94f));
            DrawCommentBubble(new Rect(width * 0.08f, height * 0.66f, 300f * scale, 78f * scale),
                "フルパワー!", scale, new Color(0.9f, 1f, 0.72f, 0.94f));
        }
    }

    void DrawSlashBand(float width, float height, float progress, float scale)
    {
        float bandProgress = 1f - Mathf.Pow(1f - Mathf.Clamp01((progress - 0.08f) / 0.18f), 3f);
        float bandWidth = width * 0.76f;
        float bandX = Mathf.Lerp(width, width * 0.12f, bandProgress);
        Rect band = new Rect(bandX, height * 0.78f, bandWidth, 42f * scale);

        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(-6f, band.center);
        DrawTinted(band, Texture2D.whiteTexture, new Color(0.15f, 1f, 0.08f, 0.58f));
        DrawTinted(new Rect(band.x, band.y + 9f * scale, band.width, 8f * scale),
            Texture2D.whiteTexture, new Color(1f, 1f, 1f, 0.9f));
        GUI.matrix = matrix;
    }

    void DrawSpecialText(float width, float height, float progress, float scale)
    {
        float textSlide = 1f - Mathf.Pow(1f - Mathf.Clamp01((progress - 0.12f) / 0.2f), 3f);
        if (textSlide <= 0f)
        {
            return;
        }

        titleStyle.fontSize = Mathf.RoundToInt(44f * scale);
        float titleX = Mathf.Lerp(-420f * scale, 38f * scale, textSlide);
        DrawOutlinedRotated(new Rect(titleX, 18f * scale, 420f * scale, 58f * scale),
            "W SPECIAL", titleStyle, new Color(0.65f, 1f, 0.3f), -4f);

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 18f) * 0.045f;
        nameStyle.fontSize = Mathf.RoundToInt(78f * scale * pulse);
        Color greenFlash = Color.Lerp(new Color(0.55f, 1f, 0.25f), Color.white, Mathf.PingPong(Time.unscaledTime * 6f, 1f));
        DrawOutlinedRotated(new Rect(0f, height - 136f * scale, width, 100f * scale),
            "メガスマッシュ!!", nameStyle, greenFlash, -2f);
    }

    void EnsureStyles()
    {
        if (titleStyle != null)
        {
            return;
        }

        titleStyle = new GUIStyle(GUI.skin.label);
        titleStyle.alignment = TextAnchor.MiddleCenter;
        titleStyle.fontStyle = FontStyle.Bold;

        nameStyle = new GUIStyle(GUI.skin.label);
        nameStyle.alignment = TextAnchor.MiddleCenter;
        nameStyle.fontStyle = FontStyle.Bold;

        commentStyle = new GUIStyle(GUI.skin.label);
        commentStyle.alignment = TextAnchor.MiddleCenter;
        commentStyle.fontStyle = FontStyle.Bold;
    }

    void DrawCommentBubble(Rect rect, string text, float scale, Color fill)
    {
        Color original = GUI.color;
        GUI.color = fill;
        GUI.DrawTexture(rect, Texture2D.whiteTexture);

        GUI.color = new Color(0.08f, 0.16f, 0.08f, 0.9f);
        GUI.DrawTexture(new Rect(rect.x, rect.y, rect.width, 4f * scale), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.yMax - 4f * scale, rect.width, 4f * scale), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.x, rect.y, 4f * scale, rect.height), Texture2D.whiteTexture);
        GUI.DrawTexture(new Rect(rect.xMax - 4f * scale, rect.y, 4f * scale, rect.height), Texture2D.whiteTexture);

        commentStyle.fontSize = Mathf.RoundToInt(34f * scale);
        commentStyle.normal.textColor = new Color(0.05f, 0.18f, 0.08f);
        GUI.Label(rect, text, commentStyle);
        GUI.color = original;
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

    static Rect CoverRect(Sprite sprite, float width, float height, float zoom)
    {
        float aspect = sprite.rect.width / sprite.rect.height;
        float screenAspect = width / height;
        float drawWidth;
        float drawHeight;

        if (aspect > screenAspect)
        {
            drawHeight = height * zoom;
            drawWidth = drawHeight * aspect;
        }
        else
        {
            drawWidth = width * zoom;
            drawHeight = drawWidth / aspect;
        }

        return new Rect((width - drawWidth) * 0.5f, (height - drawHeight) * 0.5f, drawWidth, drawHeight);
    }

    static void DrawTinted(Rect rect, Texture2D texture, Color color)
    {
        Color original = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, texture);
        GUI.color = original;
    }

    static void DrawRotated(Rect rect, Texture2D texture, Color color, float angle)
    {
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);
        DrawTinted(rect, texture, color);
        GUI.matrix = matrix;
    }

    void DrawOutlinedRotated(Rect rect, string text, GUIStyle style, Color color, float angle)
    {
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(angle, rect.center);

        style.normal.textColor = new Color(0.02f, 0.05f, 0.03f, 0.9f);
        float offset = Mathf.Max(3f, rect.height * 0.05f);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);
        GUI.Label(new Rect(rect.x - offset * 0.5f, rect.y - offset * 0.5f, rect.width, rect.height), text, style);

        style.normal.textColor = color;
        GUI.Label(rect, text, style);
        GUI.matrix = matrix;
    }

    static Texture2D GetSpeedLineTexture()
    {
        if (speedLineTexture != null)
        {
            return speedLineTexture;
        }

        const int textureWidth = 640;
        const int textureHeight = 360;
        const float rayCount = 44f;

        speedLineTexture = new Texture2D(textureWidth, textureHeight, TextureFormat.RGBA32, false);
        speedLineTexture.filterMode = FilterMode.Bilinear;
        speedLineTexture.wrapMode = TextureWrapMode.Clamp;
        Vector2 center = new Vector2(textureWidth * 0.5f, textureHeight * 0.5f);
        float maxRadius = center.magnitude;

        for (int y = 0; y < textureHeight; y++)
        {
            for (int x = 0; x < textureWidth; x++)
            {
                Vector2 delta = new Vector2(x, y) - center;
                float radius = delta.magnitude / maxRadius;
                float angle = Mathf.Atan2(delta.y, delta.x);

                float ray = Mathf.Repeat(angle / (Mathf.PI * 2f) * rayCount
                    + Mathf.PerlinNoise(Mathf.Cos(angle) * 2.3f + 4f, Mathf.Sin(angle) * 2.3f + 4f) * 1.6f, 1f);
                float thickness = 0.1f + radius * 0.3f;
                bool inRay = ray < thickness;
                float alpha = inRay ? Mathf.Clamp01((radius - 0.22f) / 0.5f) : 0f;

                speedLineTexture.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }

        speedLineTexture.Apply();
        return speedLineTexture;
    }
}
