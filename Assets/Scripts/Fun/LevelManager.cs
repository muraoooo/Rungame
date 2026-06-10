using UnityEngine;

public static class LevelManager
{
    public const int MaxStage = 4;

    public static int CurrentStage { get; private set; } = 1;

    public static bool IsFinalStage => CurrentStage >= MaxStage;
    public static string StageLabel => "1-" + CurrentStage;

    public static void SelectStage(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, 1, MaxStage);
    }

    public static void AdvanceStage()
    {
        CurrentStage = Mathf.Min(CurrentStage + 1, MaxStage);
    }

    public static void ResetToFirst()
    {
        CurrentStage = 1;
    }

    // Retry / next-stage reloads skip the title screen and go straight to play.
    static bool autoStart;

    public static void RequestAutoStart()
    {
        autoStart = true;
    }

    public static bool ConsumeAutoStart()
    {
        bool value = autoStart;
        autoStart = false;
        return value;
    }
}
