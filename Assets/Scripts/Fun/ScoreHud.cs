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
        // The opening director owns the whole title screen.
        if (!GameSession.HasStarted)
        {
            return;
        }

        EnsureStyles();
        float scale = Mathf.Clamp(Screen.height / 1080f, 0.74f, 1.2f);

        DrawScore(scale);
        DrawStageLabel(scale);
        DrawStageIntro(scale);
        DrawActionStatus(scale);
        DrawRunStatus(scale);
        DrawStartHint(scale);
        DrawEndPanel(scale);
    }

    void DrawStageLabel(float scale)
    {
        infoStyle.fontSize = Mathf.RoundToInt(24f * scale);
        Rect rect = new Rect(Screen.width - 420f * scale, (18f + 90f) * scale, 320f * scale, 32f * scale);
        DrawOutlined(rect, "STAGE " + LevelManager.StageLabel, infoStyle, new Color(0.85f, 0.95f, 0.85f));

        // Star medals: gold = grabbed this run, blue = already owned, dark = missing
        TextAnchor original = infoStyle.alignment;
        infoStyle.alignment = TextAnchor.UpperLeft;
        for (int i = 0; i < 3; i++)
        {
            Color starColor;
            if (ScoreSystem.IsMedalRunCollected(i))
            {
                starColor = new Color(1f, 0.85f, 0.25f);
            }
            else if (ScoreSystem.IsMedalOwned(LevelManager.CurrentStage, i))
            {
                starColor = new Color(0.6f, 0.8f, 1f);
            }
            else
            {
                starColor = new Color(0.4f, 0.4f, 0.45f);
            }

            Rect starRect = new Rect(Screen.width - (88f - i * 26f) * scale, (18f + 90f) * scale, 30f * scale, 32f * scale);
            DrawOutlined(starRect, "★", infoStyle, starColor);
        }
        infoStyle.alignment = original;
    }

    void DrawStageIntro(float scale)
    {
        if (!GameSession.HasStarted || GameSession.HasEnded)
        {
            return;
        }

        float elapsed = GameSession.ElapsedTime;
        if (elapsed > 1.8f)
        {
            return;
        }

        float alpha = elapsed < 1.3f ? 1f : 1f - (elapsed - 1.3f) / 0.5f;
        float popScale = 1f + Mathf.Max(0f, 0.4f - elapsed * 1.6f);
        bigStyle.fontSize = Mathf.RoundToInt(72f * scale * popScale);
        Rect rect = new Rect(0f, Screen.height * 0.3f, Screen.width, 100f * scale);
        DrawOutlined(rect, "STAGE " + LevelManager.StageLabel, bigStyle, new Color(1f, 0.95f, 0.5f, alpha));

        bigStyle.fontSize = Mathf.RoundToInt(34f * scale);
        Rect subRect = new Rect(0f, Screen.height * 0.3f + 90f * scale, Screen.width, 50f * scale);
        DrawOutlined(subRect, StageCatchphrase(), bigStyle, new Color(1f, 1f, 1f, alpha));
    }

    string StageCatchphrase()
    {
        switch (LevelManager.CurrentStage)
        {
            case 1: return "はしりだそう!";
            case 2: return "トゲに ちゅうい!";
            case 3: return "さいごの しれん!!";
            case 4: return "ひかりを たよりに すすめ…";
            default: return "キングスライムの しろ…";
        }
    }

    // A deliberately small HUD column: score, the W gauge, stage + medals.
    // Coin count and best time were cut - the score and the title screen
    // already carry that information.
    void DrawScore(float scale)
    {
        float pop = Mathf.Clamp01(1f - (Time.unscaledTime - ScoreSystem.ScorePopTime) / 0.25f);
        scoreStyle.fontSize = Mathf.RoundToInt((42f + pop * 12f) * scale);
        Rect scoreRect = new Rect(Screen.width - 420f * scale, 18f * scale, 400f * scale, 60f * scale);
        DrawOutlined(scoreRect, "SCORE " + ScoreSystem.Score.ToString("N0"), scoreStyle, new Color(1f, 0.93f, 0.45f));

        infoStyle.fontSize = Mathf.RoundToInt(24f * scale);
        Rect specialRect = new Rect(Screen.width - 420f * scale, (18f + 58f) * scale, 400f * scale, 32f * scale);
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

    void DrawActionStatus(float scale)
    {
        if (GameSession.HasEnded || GameSession.ElapsedTime <= 1.8f)
        {
            return;
        }

        string text = "";
        Color color = Color.white;
        float fontSize = 38f;
        float pulseSpeed = 6f;

        if (BossSlime.BossFightActive && BossSlime.Instance != null)
        {
            BossSlime boss = BossSlime.Instance;
            string hearts = new string('★', boss.Health) + new string('☆', boss.maxHealth - boss.Health);
            text = "キングスライム " + hearts;
            color = new Color(1f, 0.45f, 0.85f);

            if (ScoreSystem.IsFever)
            {
                text += "   FEVER " + ScoreSystem.FeverTimeLeft.ToString("0.0");
                color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.6f, 1f), 0.75f, 1f);
            }
        }
        else if (ScoreSystem.IsFever)
        {
            text = "FEVER x2  " + ScoreSystem.FeverTimeLeft.ToString("0.0");
            color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.6f, 1f), 0.75f, 1f);
            fontSize = 46f;
            pulseSpeed = 12f;
        }
        else if (ScoreSystem.Combo >= 2 && ScoreSystem.ComboTimeLeft > 0f)
        {
            text = ScoreSystem.Combo + " COMBO!";
            color = new Color(1f, 0.5f, 0.3f);
            fontSize = 42f;
            pulseSpeed = 9f;
        }

        if (string.IsNullOrEmpty(text))
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * pulseSpeed) * 0.06f;
        bigStyle.fontSize = Mathf.RoundToInt(fontSize * scale * pulse);
        Rect rect = new Rect(0f, 116f * scale, Screen.width, 54f * scale);
        DrawOutlined(rect, text, bigStyle, color);
    }

    void DrawRunStatus(float scale)
    {
        if (GameSession.HasEnded || (RespawnSystem.Deaths <= 0 && !ScoreSystem.MagnetActive))
        {
            return;
        }

        string text = "";
        if (RespawnSystem.Deaths > 0)
        {
            text = "ミス " + RespawnSystem.Deaths;
        }

        if (ScoreSystem.MagnetActive)
        {
            if (!string.IsNullOrEmpty(text))
            {
                text += "   ";
            }
            text += "マグネット " + ScoreSystem.MagnetTimeLeft.ToString("0.0");
        }

        infoStyle.fontSize = Mathf.RoundToInt(22f * scale);
        Rect rect = new Rect(Screen.width - 420f * scale, (18f + 124f) * scale, 400f * scale, 30f * scale);
        DrawOutlined(rect, text, infoStyle, new Color(0.7f, 0.95f, 1f));
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
        DrawOutlined(controlsRect, "←→:はしる SPACE:ジャンプ Q:アクロバット F:こうげき SHIFT:ダッシュ W:ひっさつ 1〜4:ステージ", hintStyle, Color.white);
    }

    void DrawEndPanel(float scale)
    {
        // The ending director owns the final-clear screen.
        if (!GameSession.HasEnded || EndingDirector.IsActive)
        {
            return;
        }

        // Bottom-anchored rows with fixed spacing:
        // ALL CLEAR > NEW RECORD > time > score/medals/misses > hint.
        string hint = "R か ENTER で もういちど!";

        if (GameSession.ReachedGoal)
        {
            DrawRankStamp(scale);
            int runMedals = 0;
            for (int i = 0; i < 3; i++)
            {
                if (ScoreSystem.IsMedalRunCollected(i))
                {
                    runMedals++;
                }
            }

            bigStyle.fontSize = Mathf.RoundToInt(34f * scale);
            Rect timeRect = new Rect(0f, Screen.height - 150f * scale, Screen.width, 42f * scale);
            DrawOutlined(timeRect, "タイム " + FormatTime(GameSession.ElapsedTime), bigStyle, Color.white);

            bigStyle.fontSize = Mathf.RoundToInt(29f * scale);
            Rect scoreRect = new Rect(0f, Screen.height - 108f * scale, Screen.width, 36f * scale);
            string scoreLine = "スコア " + ScoreSystem.Score.ToString("N0")
                + "   メダル " + runMedals + "/3"
                + (RespawnSystem.Deaths > 0 ? "   ミス " + RespawnSystem.Deaths : "   ノーミス");
            DrawOutlined(scoreRect, scoreLine, bigStyle, new Color(0.9f, 0.96f, 1f));

            if (ScoreSystem.NewRecord)
            {
                Color rainbow = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 2f, 1f), 0.8f, 1f);
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 10f) * 0.1f;
                bigStyle.fontSize = Mathf.RoundToInt(46f * scale * pulse);
                Rect recordRect = new Rect(0f, Screen.height - 214f * scale, Screen.width, 60f * scale);
                DrawOutlined(recordRect, "★ NEW RECORD!! ★", bigStyle, rainbow);
            }

            if (LevelManager.IsFinalStage)
            {
                Color gold = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.2f, 1f), 0.55f, 1f);
                float pulse = 1f + Mathf.Sin(Time.unscaledTime * 7f) * 0.08f;
                bigStyle.fontSize = Mathf.RoundToInt(52f * scale * pulse);
                Rect clearRect = new Rect(0f, Screen.height - 272f * scale, Screen.width, 64f * scale);
                DrawOutlined(clearRect, "☆ ぜんステージクリア!! ☆", bigStyle, gold);
                hint = "ENTER で 1-1 から / R でもういちど";
            }
            else
            {
                hint = "ENTER で つぎのステージへ! / R でやりなおし";
            }
        }

        float blink = 0.55f + Mathf.Sin(Time.unscaledTime * 4.5f) * 0.45f;
        hintStyle.fontSize = Mathf.RoundToInt(30f * scale);
        Rect restartRect = new Rect(0f, Screen.height - 58f * scale, Screen.width, 40f * scale);
        DrawOutlined(restartRect, hint, hintStyle, new Color(1f, 1f, 1f, blink));
    }

    void DrawRankStamp(float scale)
    {
        string rank = ScoreSystem.LastRank;
        if (string.IsNullOrEmpty(rank))
        {
            return;
        }

        float pulse = 1f + Mathf.Sin(Time.unscaledTime * 5f) * 0.05f;
        Color color = ScoreSystem.RankColor(rank);
        if (rank == "S")
        {
            color = Color.HSVToRGB(Mathf.Repeat(Time.unscaledTime * 1.4f, 1f), 0.55f, 1f);
        }

        Rect rect = new Rect(Screen.width * 0.72f, Screen.height * 0.28f, 280f * scale, 200f * scale);
        Matrix4x4 matrix = GUI.matrix;
        GUIUtility.RotateAroundPivot(-12f, rect.center);

        bigStyle.fontSize = Mathf.RoundToInt(36f * scale);
        DrawOutlined(new Rect(rect.x, rect.y, rect.width, 44f * scale), "RANK", bigStyle, color);
        bigStyle.fontSize = Mathf.RoundToInt(150f * scale * pulse);
        DrawOutlined(new Rect(rect.x, rect.y + 36f * scale, rect.width, 170f * scale), rank, bigStyle, color);

        GUI.matrix = matrix;
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
