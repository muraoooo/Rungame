using UnityEngine;

public static class LevelManager
{
    public const int MaxStage = 5;

    public static int CurrentStage { get; private set; } = 1;

    public static bool IsFinalStage => CurrentStage >= MaxStage;
    public static string StageLabel => "1-" + CurrentStage;

    public static void SelectStage(int stage)
    {
        CurrentStage = Mathf.Clamp(stage, 1, MaxStage);
    }

    public static string SceneNameForStage(int stage)
    {
        return "Stage" + Mathf.Clamp(stage, 1, MaxStage);
    }

    // Lets each stage live in its own scene: opening Stage3.unity in the
    // editor and pressing Play starts stage 1-3 directly.
    public static void DetectStageFromScene(string sceneName)
    {
        if (string.IsNullOrEmpty(sceneName) || !sceneName.StartsWith("Stage"))
        {
            return;
        }

        int stage;
        if (int.TryParse(sceneName.Substring("Stage".Length), out stage))
        {
            CurrentStage = Mathf.Clamp(stage, 1, MaxStage);
        }
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
