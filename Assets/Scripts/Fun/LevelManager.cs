using UnityEngine;

public static class LevelManager
{
    public const int MaxStage = 3;

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
}
