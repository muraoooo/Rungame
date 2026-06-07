using UnityEngine;

public class TimeAttackTimer : MonoBehaviour
{
    public Vector2 screenPosition = new Vector2(24f, 20f);
    public int fontSize = 32;

    GUIStyle labelStyle;
    GUIStyle timeStyle;
    GUIStyle miniStyle;
    Texture2D whiteTexture;
    Texture2D panelTexture;
    Texture2D panelBorderTexture;
    Texture2D ribbonTexture;
    Texture2D clockFaceTexture;
    Texture2D clockRingTexture;
    Texture2D starTexture;
    Texture2D leafTexture;

    readonly Color skyBlue = new Color(0.45f, 0.82f, 1f, 1f);
    readonly Color mintGreen = new Color(0.34f, 0.86f, 0.56f, 1f);
    readonly Color finishGold = new Color(1f, 0.79f, 0.2f, 1f);
    readonly Color berryPink = new Color(1f, 0.43f, 0.58f, 1f);
    readonly Color inkColor = new Color(0.12f, 0.16f, 0.22f, 1f);

    void OnGUI()
    {
        EnsureResources();

        float scale = Mathf.Clamp(Screen.height / 1080f, 0.74f, 1.2f);
        Vector2 position = screenPosition * scale;
        float panelWidth = 304f * scale;
        float panelHeight = 92f * scale;
        Color accentColor = GameSession.ReachedGoal ? finishGold : skyBlue;

        Rect shadowRect = new Rect(position.x + 5f * scale, position.y + 6f * scale, panelWidth, panelHeight);
        Rect panelRect = new Rect(position.x, position.y, panelWidth, panelHeight);
        Rect borderRect = new Rect(position.x - 3f * scale, position.y - 3f * scale, panelWidth + 6f * scale, panelHeight + 6f * scale);
        float labelBounce = Mathf.Sin(Time.realtimeSinceStartup * 4.2f) * 2f * scale;
        float labelSquish = 1f + Mathf.Sin(Time.realtimeSinceStartup * 4.2f) * 0.035f;
        Rect ribbonRect = new Rect(position.x + 72f * scale, position.y - 5f * scale + labelBounce, 108f * scale, 24f * scale);
        Rect clockRect = new Rect(position.x + 15f * scale, position.y + 23f * scale, 48f * scale, 48f * scale);

        DrawTinted(shadowRect, panelTexture, new Color(0f, 0f, 0f, 0.22f));
        DrawTinted(borderRect, panelBorderTexture, accentColor);
        DrawTinted(panelRect, panelTexture, Color.white);
        DrawPulsingRibbon(ribbonRect, GameSession.ReachedGoal ? finishGold : mintGreen, labelSquish);
        DrawDecorations(position, scale, accentColor);
        DrawClock(clockRect, scale, accentColor);

        string label = GameSession.ReachedGoal ? "FINISH!" : "TIME";
        string mini = GameSession.ReachedGoal ? "Best ride!" : "YUNKEY RUN";
        string timeText = FormatTime(GameSession.ElapsedTime);

        labelStyle.fontSize = Mathf.RoundToInt(fontSize * 0.43f * scale);
        timeStyle.fontSize = Mathf.RoundToInt(fontSize * 1.28f * scale);
        miniStyle.fontSize = Mathf.RoundToInt(fontSize * 0.3f * scale);
        timeStyle.normal.textColor = GameSession.ReachedGoal ? new Color(0.55f, 0.32f, 0.04f, 1f) : inkColor;

        DrawCuteLabel(new Rect(ribbonRect.x, ribbonRect.y + 2f * scale, ribbonRect.width, ribbonRect.height), label, scale);
        DrawCuteTime(new Rect(position.x + 76f * scale, position.y + 24f * scale, 212f * scale, 48f * scale), timeText, scale);
        GUI.Label(new Rect(position.x + 82f * scale, position.y + 67f * scale, 154f * scale, 18f * scale), mini, miniStyle);
    }

    void EnsureResources()
    {
        if (whiteTexture == null)
        {
            whiteTexture = Texture2D.whiteTexture;
            panelTexture = CreateRoundedRectTexture(96, 40, 18, new Color(0.98f, 0.97f, 0.88f, 0.95f));
            panelBorderTexture = CreateRoundedRectTexture(100, 44, 20, Color.white);
            ribbonTexture = CreateRoundedRectTexture(80, 28, 12, Color.white);
            clockFaceTexture = CreateCircleTexture(72, new Color(1f, 0.98f, 0.86f, 1f));
            clockRingTexture = CreateRingTexture(80, 0.75f, Color.white);
            starTexture = CreateStarTexture(48, Color.white);
            leafTexture = CreateLeafTexture(48, Color.white);
        }

        if (labelStyle != null)
        {
            return;
        }

        labelStyle = new GUIStyle(GUI.skin.label);
        labelStyle.alignment = TextAnchor.MiddleCenter;
        labelStyle.fontStyle = FontStyle.Bold;
        labelStyle.normal.textColor = Color.white;

        timeStyle = new GUIStyle(GUI.skin.label);
        timeStyle.alignment = TextAnchor.MiddleLeft;
        timeStyle.fontStyle = FontStyle.Bold;

        miniStyle = new GUIStyle(GUI.skin.label);
        miniStyle.alignment = TextAnchor.MiddleLeft;
        miniStyle.fontStyle = FontStyle.Bold;
        miniStyle.normal.textColor = new Color(0.31f, 0.42f, 0.37f, 1f);
    }

    void DrawDecorations(Vector2 position, float scale, Color accentColor)
    {
        DrawTinted(new Rect(position.x + 260f * scale, position.y + 10f * scale, 20f * scale, 20f * scale), starTexture, finishGold);
        DrawTinted(new Rect(position.x + 278f * scale, position.y + 58f * scale, 16f * scale, 16f * scale), starTexture, berryPink);
        DrawTinted(new Rect(position.x + 216f * scale, position.y + 72f * scale, 22f * scale, 14f * scale), leafTexture, mintGreen);
        DrawTinted(new Rect(position.x + 10f * scale, position.y + 72f * scale, 18f * scale, 12f * scale), leafTexture, accentColor);
    }

    void DrawClock(Rect rect, float scale, Color accentColor)
    {
        DrawTinted(rect, clockRingTexture, accentColor);
        DrawTinted(new Rect(rect.x + 5f * scale, rect.y + 5f * scale, rect.width - 10f * scale, rect.height - 10f * scale), clockFaceTexture, Color.white);

        Vector2 center = rect.center;
        DrawLine(center, center + new Vector2(0f, -12f * scale), 3f * scale, inkColor);
        DrawLine(center, center + new Vector2(10f * scale, 7f * scale), 3f * scale, inkColor);
        DrawTinted(new Rect(center.x - 3f * scale, center.y - 3f * scale, 6f * scale, 6f * scale), clockFaceTexture, berryPink);
        DrawTinted(new Rect(rect.x + 15f * scale, rect.y - 3f * scale, 8f * scale, 10f * scale), leafTexture, mintGreen);
    }

    void DrawPulsingRibbon(Rect rect, Color color, float scalePulse)
    {
        Matrix4x4 originalMatrix = GUI.matrix;
        Vector2 pivot = rect.center;
        GUIUtility.ScaleAroundPivot(new Vector2(scalePulse, 1f / scalePulse), pivot);
        DrawTinted(rect, ribbonTexture, color);
        GUI.matrix = originalMatrix;
    }

    void DrawCuteLabel(Rect rect, string text, float scale)
    {
        Color originalColor = labelStyle.normal.textColor;
        Color sparkleColor = new Color(1f, 1f, 1f, 0.55f + Mathf.Sin(Time.realtimeSinceStartup * 6f) * 0.18f);

        labelStyle.normal.textColor = new Color(0.18f, 0.36f, 0.32f, 0.22f);
        GUI.Label(new Rect(rect.x + 1f * scale, rect.y + 2f * scale, rect.width, rect.height), text, labelStyle);

        labelStyle.normal.textColor = sparkleColor;
        GUI.Label(new Rect(rect.x, rect.y - 0.6f * scale, rect.width, rect.height), text, labelStyle);

        labelStyle.normal.textColor = originalColor;
        GUI.Label(rect, text, labelStyle);
    }

    void DrawCuteTime(Rect rect, string text, float scale)
    {
        Color mainColor = timeStyle.normal.textColor;
        Color shadow = GameSession.ReachedGoal ? new Color(1f, 0.9f, 0.44f, 0.85f) : new Color(0.67f, 0.89f, 1f, 0.78f);
        Color shine = new Color(1f, 1f, 1f, 0.45f);

        timeStyle.normal.textColor = shadow;
        GUI.Label(new Rect(rect.x + 1f * scale, rect.y + 2f * scale, rect.width, rect.height), text, timeStyle);

        timeStyle.normal.textColor = shine;
        GUI.Label(new Rect(rect.x - 0.45f * scale, rect.y - 0.45f * scale, rect.width, rect.height), text, timeStyle);

        timeStyle.normal.textColor = mainColor;
        GUI.Label(rect, text, timeStyle);
    }

    void DrawLine(Vector2 start, Vector2 end, float width, Color color)
    {
        Matrix4x4 originalMatrix = GUI.matrix;
        Color originalColor = GUI.color;
        float angle = Mathf.Atan2(end.y - start.y, end.x - start.x) * Mathf.Rad2Deg;
        float length = Vector2.Distance(start, end);

        GUI.color = color;
        GUIUtility.RotateAroundPivot(angle, start);
        GUI.DrawTexture(new Rect(start.x, start.y - width * 0.5f, length, width), whiteTexture);
        GUI.matrix = originalMatrix;
        GUI.color = originalColor;
    }

    void DrawTinted(Rect rect, Texture2D texture, Color color)
    {
        Color originalColor = GUI.color;
        GUI.color = color;
        GUI.DrawTexture(rect, texture);
        GUI.color = originalColor;
    }

    Texture2D CreateRoundedRectTexture(int width, int height, int radius, Color color)
    {
        Texture2D texture = new Texture2D(width, height, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                float dx = Mathf.Max(radius - x, 0, x - (width - radius - 1));
                float dy = Mathf.Max(radius - y, 0, y - (height - radius - 1));
                float alpha = dx * dx + dy * dy <= radius * radius ? color.a : 0f;
                texture.SetPixel(x, y, new Color(color.r, color.g, color.b, alpha));
            }
        }

        texture.Apply();
        return texture;
    }

    Texture2D CreateCircleTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        float radius = center;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                texture.SetPixel(x, y, distance <= radius ? color : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    Texture2D CreateRingTexture(int size, float innerRatio, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        float center = (size - 1) * 0.5f;
        float outerRadius = center;
        float innerRadius = center * innerRatio;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), new Vector2(center, center));
                texture.SetPixel(x, y, distance <= outerRadius && distance >= innerRadius ? color : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    Texture2D CreateStarTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2(x, y) - center;
                float angle = Mathf.Atan2(point.y, point.x);
                float radius = 10f + 7f * Mathf.Abs(Mathf.Sin(angle * 4f));
                texture.SetPixel(x, y, point.magnitude <= radius ? color : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    Texture2D CreateLeafTexture(int size, Color color)
    {
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(size * 0.5f, size * 0.5f);

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                Vector2 point = new Vector2((x - center.x) / (size * 0.42f), (y - center.y) / (size * 0.24f));
                bool inside = point.x * point.x + point.y * point.y <= 1f && x > size * 0.14f;
                texture.SetPixel(x, y, inside ? color : Color.clear);
            }
        }

        texture.Apply();
        return texture;
    }

    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remainingSeconds = seconds - minutes * 60f;
        return minutes.ToString("00") + ":" + remainingSeconds.ToString("00.00");
    }
}
