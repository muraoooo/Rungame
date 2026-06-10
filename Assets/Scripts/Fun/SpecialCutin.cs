using UnityEngine;

// Full-screen manga-style cut-in for the W special attack.
// If a sprite exists at Resources/UI/SpecialCutin it is used as the artwork;
// otherwise the player sprite is blown up over procedural speed lines.
public class SpecialCutin : MonoBehaviour
{
    public static bool IsPlaying { get; private set; }

    const float Duration = 1.25f;
    const float BlastRadius = 16f;

    static Texture2D speedLineTexture;

    GameObject player;
    Sprite cutinSprite;
    Sprite playerSprite;
    float startTime;
    bool unleashed;
    GUIStyle titleStyle;
    GUIStyle nameStyle;

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

        if (cutinSprite != null)
        {
            float aspect = cutinSprite.rect.width / cutinSprite.rect.height;
            float artWidth = artHeight * aspect;
            float artX = Mathf.Lerp(-artWidth, (width - artWidth) * 0.5f, slide) + Mathf.Max(0f, drift);
            DrawSprite(new Rect(artX, (height - artHeight) * 0.45f, artWidth, artHeight), cutinSprite, Color.white);
        }
        else if (playerSprite != null)
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
