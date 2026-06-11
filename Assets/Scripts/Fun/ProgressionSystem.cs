using UnityEngine;

public static class ProgressionSystem
{
    const int CoinPowerStage = 2;
    const int SpecialPowerStage = 3;
    const int GuardPowerStage = 4;

    public static bool HasPendingUnlock { get; private set; }
    public static string PendingUnlockName { get; private set; } = "";
    public static int PendingUnlockStage { get; private set; }

    public static float CoinPullBonus => IsStageUnlocked(CoinPowerStage) ? 1.5f : 1f;
    public static int SpecialChargeMax => IsStageUnlocked(SpecialPowerStage) ? 4 : 5;
    public static float RespawnInvulnerabilitySeconds => IsStageUnlocked(GuardPowerStage) ? 3f : 2f;

    public static int UnlockedCount
    {
        get
        {
            int count = 0;
            if (IsStageUnlocked(CoinPowerStage))
            {
                count++;
            }
            if (IsStageUnlocked(SpecialPowerStage))
            {
                count++;
            }
            if (IsStageUnlocked(GuardPowerStage))
            {
                count++;
            }
            return count;
        }
    }

    public static bool RegisterStageClear(int stage)
    {
        HasPendingUnlock = false;
        PendingUnlockName = "";
        PendingUnlockStage = 0;

        if (!IsUnlockStage(stage) || IsStageUnlocked(stage))
        {
            return false;
        }

        PlayerPrefs.SetInt(KeyForStage(stage), 1);
        PlayerPrefs.Save();

        HasPendingUnlock = true;
        PendingUnlockStage = stage;
        PendingUnlockName = PowerNameForStage(stage);
        return true;
    }

    public static bool IsStageUnlocked(int stage)
    {
        return PlayerPrefs.GetInt(KeyForStage(stage), 0) == 1;
    }

    public static string PowerNameForStage(int stage)
    {
        switch (stage)
        {
            case CoinPowerStage: return "コインのちから";
            case SpecialPowerStage: return "ひっさつのちから";
            case GuardPowerStage: return "まもりのちから";
            default: return "";
        }
    }

    static bool IsUnlockStage(int stage)
    {
        return stage == CoinPowerStage || stage == SpecialPowerStage || stage == GuardPowerStage;
    }

    static string KeyForStage(int stage)
    {
        return "RungameUnlock_S" + stage;
    }
}
