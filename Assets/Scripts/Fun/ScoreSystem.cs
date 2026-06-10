using UnityEngine;

public static class ScoreSystem
{
    const string BestTimeKey = "RungameBestTime";
    const float ComboWindowSeconds = 3.5f;
    const float FeverSeconds = 7f;

    public const int SpecialChargeMax = 5;

    public static int Score { get; private set; }
    public static int Combo { get; private set; }
    public static int CoinsCollected { get; private set; }
    public static int SpecialCharge { get; private set; }
    public static bool SpecialReady => SpecialCharge >= SpecialChargeMax;
    public static bool NewRecord { get; private set; }
    public static float ScorePopTime { get; private set; }

    static float comboEndTime;
    static float feverEndTime;
    static float magnetEndTime;

    public static bool IsFever => GameSession.HasStarted && Time.time < feverEndTime;
    public static bool MagnetActive => GameSession.HasStarted && !GameSession.HasEnded && Time.time < magnetEndTime;
    public static float MagnetTimeLeft => Mathf.Max(0f, magnetEndTime - Time.time);
    public static float FeverTimeLeft => Mathf.Max(0f, feverEndTime - Time.time);
    public static float ComboTimeLeft => Mathf.Max(0f, comboEndTime - Time.time);
    public static float SpeedMultiplier => IsFever && !GameSession.HasEnded ? 1.3f : 1f;
    public static float BestTime => PlayerPrefs.GetFloat(BestTimeKeyForStage(), 0f);

    public static void Reset()
    {
        Score = 0;
        Combo = 0;
        CoinsCollected = 0;
        SpecialCharge = 0;
        NewRecord = false;
        comboEndTime = 0f;
        feverEndTime = 0f;
        magnetEndTime = 0f;
        ScorePopTime = -10f;
    }

    public static void AddCoin(Vector3 position)
    {
        CoinsCollected++;
        bool wasReady = SpecialReady;
        SpecialCharge = Mathf.Min(SpecialChargeMax, SpecialCharge + 1);
        int points = 100 * (IsFever ? 2 : 1);
        AddScore(points);
        JuiceManager.Popup(position, "+" + points, new Color(1f, 0.84f, 0.25f), 0.95f);

        if (SpecialReady && !wasReady)
        {
            JuiceManager.Popup(position + Vector3.up * 0.9f, "W で ひっさつ READY!", new Color(0.6f, 1f, 0.45f), 1.1f);
        }
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

    public static bool TryConsumeSpecial()
    {
        if (!SpecialReady)
        {
            return false;
        }

        SpecialCharge = 0;
        return true;
    }

    public static void AddBonus(int points, Vector3 position, string label)
    {
        AddScore(points);
        JuiceManager.Popup(position, label + " +" + points, new Color(1f, 0.9f, 0.3f), 1.3f);
    }

    public static void ActivateMagnet(float seconds)
    {
        magnetEndTime = Time.time + seconds;
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
            PlayerPrefs.SetFloat(BestTimeKeyForStage(), clearTime);
            PlayerPrefs.Save();
            NewRecord = true;
        }
    }

    static string BestTimeKeyForStage()
    {
        return BestTimeKey + "_S" + LevelManager.CurrentStage;
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
