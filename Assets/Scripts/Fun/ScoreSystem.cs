using UnityEngine;

public static class ScoreSystem
{
    const string BestTimeKey = "RungameBestTime";
    const float ComboWindowSeconds = 3.5f;
    const float FeverSeconds = 7f;

    public static int Score { get; private set; }
    public static int Combo { get; private set; }
    public static int CoinsCollected { get; private set; }
    public static bool NewRecord { get; private set; }
    public static float ScorePopTime { get; private set; }

    static float comboEndTime;
    static float feverEndTime;

    public static bool IsFever => GameSession.HasStarted && Time.time < feverEndTime;
    public static float FeverTimeLeft => Mathf.Max(0f, feverEndTime - Time.time);
    public static float ComboTimeLeft => Mathf.Max(0f, comboEndTime - Time.time);
    public static float SpeedMultiplier => IsFever && !GameSession.HasEnded ? 1.3f : 1f;
    public static float BestTime => PlayerPrefs.GetFloat(BestTimeKey, 0f);

    public static void Reset()
    {
        Score = 0;
        Combo = 0;
        CoinsCollected = 0;
        NewRecord = false;
        comboEndTime = 0f;
        feverEndTime = 0f;
        ScorePopTime = -10f;
    }

    public static void AddCoin(Vector3 position)
    {
        CoinsCollected++;
        int points = 100 * (IsFever ? 2 : 1);
        AddScore(points);
        JuiceManager.Popup(position, "+" + points, new Color(1f, 0.84f, 0.25f), 0.95f);
    }

    public static void RegisterStomp(Vector3 position)
    {
        if (Time.time > comboEndTime)
        {
            Combo = 0;
        }

        Combo++;
        comboEndTime = Time.time + ComboWindowSeconds;

        int points = 200 * Combo * (IsFever ? 2 : 1);
        AddScore(points);
        JuiceManager.Popup(position, "+" + points, Color.white, 1f);

        if (Combo >= 2)
        {
            JuiceManager.Popup(position + Vector3.up * 0.9f, Combo + " COMBO!", new Color(1f, 0.48f, 0.32f), 1.2f);
        }

        if (Combo >= 3 && !IsFever)
        {
            StartFever(position);
        }
    }

    public static void AddTrick(Vector3 position)
    {
        int points = 50 * (IsFever ? 2 : 1);
        AddScore(points);
        JuiceManager.Popup(position + Vector3.up * 0.6f, "TRICK +" + points, new Color(0.45f, 0.9f, 1f), 0.9f);
    }

    public static void OnGoal(float clearTime)
    {
        int timeBonus = Mathf.Max(0, Mathf.RoundToInt((90f - clearTime) * 25f));
        if (timeBonus > 0)
        {
            AddScore(timeBonus);
        }

        float best = BestTime;
        if (best <= 0f || clearTime < best)
        {
            PlayerPrefs.SetFloat(BestTimeKey, clearTime);
            PlayerPrefs.Save();
            NewRecord = true;
        }
    }

    static void StartFever(Vector3 position)
    {
        feverEndTime = Time.time + FeverSeconds;
        JuiceManager.Popup(position + Vector3.up * 1.8f, "FEVER!!", new Color(1f, 0.35f, 0.9f), 1.7f);
        JuiceManager.Shake(0.35f);
        RetroSfx.PlayFever();
    }

    static void AddScore(int points)
    {
        Score += points;
        ScorePopTime = Time.unscaledTime;
    }
}
