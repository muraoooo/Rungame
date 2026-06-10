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
    public static string LastRank { get; private set; } = "";
    public static float ScorePopTime { get; private set; }

    // Par clear times per stage (index = stage number)
    static readonly float[] parTimes = { 0f, 35f, 45f, 60f, 65f, 75f };

    static float comboEndTime;
    static float feverEndTime;
    static float magnetEndTime;
    static bool[] medalsThisRun = new bool[3];

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
        LastRank = "";
        comboEndTime = 0f;
        feverEndTime = 0f;
        magnetEndTime = 0f;
        medalsThisRun = new bool[3];
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

    public static void BreakCombo()
    {
        Combo = 0;
        comboEndTime = 0f;
        feverEndTime = 0f;
    }

    public static void ActivateMagnet(float seconds)
    {
        magnetEndTime = Time.time + seconds;
    }

    // --- Star medals (kept only when the stage is cleared) ---

    public static bool IsMedalRunCollected(int index)
    {
        return index >= 0 && index < medalsThisRun.Length && medalsThisRun[index];
    }

    public static bool IsMedalOwned(int stage, int index)
    {
        return (PlayerPrefs.GetInt("RungameMedals_S" + stage, 0) & (1 << index)) != 0;
    }

    public static int OwnedMedalCount(int stage)
    {
        int mask = PlayerPrefs.GetInt("RungameMedals_S" + stage, 0);
        int count = 0;
        for (int i = 0; i < 3; i++)
        {
            if ((mask & (1 << i)) != 0)
            {
                count++;
            }
        }
        return count;
    }

    public static int TotalMedals
    {
        get
        {
            int total = 0;
            for (int stage = 1; stage <= LevelManager.MaxStage; stage++)
            {
                total += OwnedMedalCount(stage);
            }
            return total;
        }
    }

    public static void CollectMedal(int index, bool ownedBefore, Vector3 position)
    {
        if (index >= 0 && index < medalsThisRun.Length)
        {
            medalsThisRun[index] = true;
        }

        AddScore(500);
        RetroSfx.PlayMedal();
        JuiceManager.Burst(position, new Color(1f, 0.88f, 0.35f), 14, 6f);
        JuiceManager.Shake(0.15f);
        JuiceManager.Popup(position + Vector3.up * 0.8f,
            ownedBefore ? "+500" : "スターメダル!!", new Color(1f, 0.88f, 0.35f), 1.3f);
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
            NewRecord = true;
        }

        // Rank: S = under par with no deaths, A = under par,
        // B = under 1.4x par, C = finished. "One more run" fuel.
        int stage = Mathf.Clamp(LevelManager.CurrentStage, 1, parTimes.Length - 1);
        float par = parTimes[stage];
        int rankValue;
        if (clearTime <= par && RespawnSystem.Deaths == 0)
        {
            rankValue = 3;
        }
        else if (clearTime <= par)
        {
            rankValue = 2;
        }
        else if (clearTime <= par * 1.4f)
        {
            rankValue = 1;
        }
        else
        {
            rankValue = 0;
        }

        LastRank = RankName(rankValue);

        string rankKey = "RungameBestRank_S" + stage;
        if (rankValue > PlayerPrefs.GetInt(rankKey, -1))
        {
            PlayerPrefs.SetInt(rankKey, rankValue);
        }

        // Star-coin rule: medals are only kept when you reach the goal
        string medalKey = "RungameMedals_S" + stage;
        int medalMask = PlayerPrefs.GetInt(medalKey, 0);
        for (int i = 0; i < medalsThisRun.Length; i++)
        {
            if (medalsThisRun[i])
            {
                medalMask |= 1 << i;
            }
        }
        PlayerPrefs.SetInt(medalKey, medalMask);

        PlayerPrefs.Save();
    }

    public static string BestRank(int stage)
    {
        return RankName(PlayerPrefs.GetInt("RungameBestRank_S" + stage, -1));
    }

    public static Color RankColor(string rank)
    {
        switch (rank)
        {
            case "S": return new Color(1f, 0.85f, 0.2f);
            case "A": return new Color(1f, 0.45f, 0.4f);
            case "B": return new Color(0.5f, 0.8f, 1f);
            case "C": return new Color(0.75f, 0.75f, 0.78f);
            default: return Color.white;
        }
    }

    static string RankName(int value)
    {
        switch (value)
        {
            case 3: return "S";
            case 2: return "A";
            case 1: return "B";
            case 0: return "C";
            default: return "";
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
