using UnityEngine;

public class ScoreHud : MonoBehaviour
{
    GUIStyle scoreStyle;
    GUIStyle infoStyle;
    GUIStyle bigStyle;
    GUIStyle hintStyle;

    void EnsureStyles()
    {
        if (scoreStyle != null)
        {
            return;
        }

        scoreStyle = new GUIStyle(GUI.skin.label);
        scoreStyle.alignment = TextAnchor.UpperRight;
        scoreStyle.fontStyle = FontStyle.Bold;

        infoStyle = new GUIStyle(GUI.skin.label);
        infoStyle.alignment = TextAnchor.UpperRight;
        infoStyle.fontStyle = FontStyle.Bold;

        bigStyle = new GUIStyle(GUI.skin.label);
        bigStyle.alignment = TextAnchor.MiddleCenter;
        bigStyle.fontStyle = FontStyle.Bold;

        hintStyle = new GUIStyle(GUI.skin.label);
        hintStyle.alignment = TextAnchor.MiddleCenter;
        hintStyle.fontStyle = FontStyle.Bold;
    }

    void OnGUI()
    {
        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.74f, 1.2f);

        DrawScore(scale);
        DrawCombo(scale);
        DrawFever(scale);
        DrawStartHint(scale);
        DrawEndPanel(scale);
    }

    void DrawScore(float scale)
    {
        float pop = Mathf.Clamp01(1f - (Time.unscaledTime - ScoreSystem.ScorePopTime) / 0.25f);
        scoreStyle.fontSize = Mathf.RoundToInt((42f + pop * 12f) * scale);
        Rect scoreRect = new Rect(Screen.width - 420f * scale, 18f * scale, 400f * scale, 60f * scale);
        DrawOutlined(scoreRect, "SCORE " + ScoreSystem.Score.ToString("N0"), scoreStyle, new Color(1f, 0.93f, 0.45f));

        infoStyle.fontSize = Mathf.RoundToInt(24f * scale);
        Rect coinRect = new Rect(Screen.width - 420f * scale, (18f + 58f) * scale, 400f * scale, 32f * scale);
        DrawOutlined(coinRect, "COIN x " + ScoreSystem.CoinsCollected, infoStyle, new Color(1f, 0.8f, 0.3f));

        float best = ScoreSystem.BestTime;
        if (best > 0f)
        {
            Rect bestRect = new Rect(Screen.width - 420f * scale, (18f + 90f) * scale, 400f * scale, 30f * scale);
            DrawOutlined(bestRect, "BEST " + FormatTime(best), infoStyle, new Color(0.7f, 0.95f, 1f));
        }

        infoStyle.fontSize = Mathf.RoundToInt(24f * scale);
        Rect specialRect = new Rect(Screen.width - 420f * scale, (18f + 122f) * scale, 400f * scale, 32f * scale);
        if (ScoreSystem.SpecialReady)
        {
            Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.8f, 1f), 0.7f, 1f);
            DrawOutlined(specialRect, "W: ひっさつ READY!!", infoStyle, rainbow);
        }
        else
        {
            string gauge = new string('★', ScoreSystem.SpecialCharge)
                + new string('☆', ScoreSystem.SpecialChargeMax - ScoreSystem.SpecialCharge);
            DrawOutlined(specialRect, "W: " + gauge, infoStyle, new Color(0.75f, 0.8f, 0.85f));
        }
    }

    void DrawCombo(float scale)
    {
        if (GameSession.HasEnded || ScoreSystem.Combo < 2 || ScoreSystem.ComboTimeLeft <= 0f)
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 9f) * 0.06f;
        bigStyle.fontSize = Mathf.RoundToInt(46f * scale * pulse);
        Rect rect = new Rect(0f, 116f * scale, Screen.width, 60f * scale);
        DrawOutlined(rect, ScoreSystem.Combo + " COMBO!", bigStyle, new Color(1f, 0.5f, 0.3f));
    }

    void DrawFever(float scale)
    {
        if (!ScoreSystem.IsFever || GameSession.HasEnded)
        {
            return;
        }

        Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.6f, 1f), 0.75f, 1f);
        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 12f) * 0.1f;
        bigStyle.fontSize = Mathf.RoundToInt(58f * scale * pulse);
        Rect rect = new Rect(0f, 30f * scale, Screen.width, 76f * scale);
        DrawOutlined(rect, "FEVER!! x2", bigStyle, rainbow);

        bigStyle.fontSize = Mathf.RoundToInt(24f * scale);
        Rect timeRect = new Rect(0f, 100f * scale, Screen.width, 30f * scale);
        DrawOutlined(timeRect, ScoreSystem.FeverTimeLeft.ToString("0.0"), bigStyle, Color.white);
    }

    void DrawStartHint(float scale)
    {
        if (GameSession.HasStarted)
        {
            return;
        }

        hintStyle.fontSize = Mathf.RoundToInt(27f * scale);
        Rect startRect = new Rect(0f, Screen.height - 104f * scale, Screen.width, 36f * scale);
        float blink = 0.6f + Mathf.Sin(Time.unscaledTime * 5f) * 0.4f;
        DrawOutlined(startRect, "SPACE でスタート!", hintStyle, new Color(1f, 0.95f, 0.5f, blink));

        hintStyle.fontSize = Mathf.RoundToInt(20f * scale);
        Rect controlsRect = new Rect(0f, Screen.height - 56f * scale, Screen.width, 30f * scale);
        DrawOutlined(controlsRect, "←→:はしる SPACE:ジャンプ Q:アクロバット F:こうげき SHIFT:ダッシュ W:ひっさつ", hintStyle, Color.white);
    }

    void DrawEndPanel(float scale)
    {
        if (!GameSession.HasEnded)
        {
            return;
        }

        // Bottom-anchored rows with fixed spacing so nothing overlaps:
        // NEW RECORD (height-190) > time/score (height-124) > restart hint (height-58).
        if (GameSession.ReachedGoal)
        {
            bigStyle.fontSize = Mathf.RoundToInt(34f * scale);
            Rect timeRect = new Rect(0f, Screen.height - 124f * scale, Screen.width, 46f * scale);
            DrawOutlined(timeRect, "タイム " + FormatTime(GameSession.ElapsedTime) + "   スコア " + ScoreSystem.Score.ToString("N0"), bigStyle, Color.white);

            if (ScoreSystem.NewRecord)
            {
                Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 2f, 1f), 0.8f, 1f);
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.1f;
                bigStyle.fontSize = Mathf.RoundToInt(46f * scale * pulse);
                Rect recordRect = new Rect(0f, Screen.height - 190f * scale, Screen.width, 60f * scale);
                DrawOutlined(recordRect, "★ NEW RECORD!! ★", bigStyle, rainbow);
            }
        }

        float blink = 0.55f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.45f;
        hintStyle.fontSize = Mathf.RoundToInt(30f * scale);
        Rect restartRect = new Rect(0f, Screen.height - 58f * scale, Screen.width, 40f * scale);
        DrawOutlined(restartRect, "R か ENTER で もういちど!", hintStyle, new Color(1f, 1f, 1f, blink));
    }

    void DrawOutlined(Rect rect, string text, GUIStyle style, Color color)
    {
        Color original = style.normal.textColor;
        float offset = Mathf.Max(2f, rect.height * 0.045f);

        style.normal.textColor = new Color(0.05f, 0.08f, 0.12f, 0.8f * color.a);
        GUI.Label(new Rect(rect.x + offset, rect.y + offset, rect.width, rect.height), text, style);

        style.normal.textColor = color;
        GUI.Label(rect, text, style);
        style.normal.textColor = original;
    }

    string FormatTime(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60f);
        float remaining = seconds - minutes * 60f;
        return minutes.ToString("00") + ":" + remaining.ToString("00.00");
    }
}
