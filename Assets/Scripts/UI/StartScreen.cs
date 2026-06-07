using UnityEngine;
using UnityEngine.InputSystem;

public class StartScreen : MonoBehaviour
{
    public string startBannerResourcePath = "UI/StartBanner";

    bool hasStarted;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void CreateIfMissing()
    {
        if (Object.FindAnyObjectByType<StartScreen>() != null)
        {
            return;
        }

        new GameObject("StartScreen").AddComponent<StartScreen>();
    }

    void Start()
    {
        GameSession.WaitForStart();
        EndBannerUI.Show(startBannerResourcePath);
    }

    void Update()
    {
        if (hasStarted)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        Mouse mouse = Mouse.current;

        bool pressedStart = false;

        if (keyboard != null)
        {
            pressedStart = keyboard.spaceKey.wasPressedThisFrame
                || keyboard.enterKey.wasPressedThisFrame
                || keyboard.numpadEnterKey.wasPressedThisFrame;
        }

        if (mouse != null && mouse.leftButton.wasPressedThisFrame)
        {
            pressedStart = true;
        }

        if (!pressedStart)
        {
            return;
        }

        hasStarted = true;
        EndBannerUI.Clear();
        GameSession.StartGame();
    }
}
