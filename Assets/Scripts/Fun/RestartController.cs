using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class RestartController : MonoBehaviour
{
    public float fallLimitY = -12f;

    Transform player;
    bool fellHandled;

    void Update()
    {
        Keyboard keyboard = Keyboard.current;

        if (keyboard != null && keyboard.rKey.wasPressedThisFrame && GameSession.HasStarted)
        {
            Restart();
            return;
        }

        // ESC returns to the title screen
        if (keyboard != null && keyboard.escapeKey.wasPressedThisFrame && GameSession.HasStarted)
        {
            Time.timeScale = 1f;
            EndBannerUI.Clear();
            LoadStageScene();
            return;
        }

        if (keyboard != null && GameSession.HasEnded
            && (keyboard.enterKey.wasPressedThisFrame || keyboard.numpadEnterKey.wasPressedThisFrame))
        {
            if (GameSession.ReachedGoal)
            {
                if (LevelManager.IsFinalStage)
                {
                    LevelManager.ResetToFirst();
                }
                else
                {
                    LevelManager.AdvanceStage();
                }
            }

            Restart();
            return;
        }

        // Stage select on the title screen
        if (keyboard != null && !GameSession.HasStarted)
        {
            int selected = 0;
            if (keyboard.digit1Key.wasPressedThisFrame) selected = 1;
            if (keyboard.digit2Key.wasPressedThisFrame) selected = 2;
            if (keyboard.digit3Key.wasPressedThisFrame) selected = 3;
            if (keyboard.digit4Key.wasPressedThisFrame) selected = 4;
            if (keyboard.digit5Key.wasPressedThisFrame) selected = 5;

            if (selected > 0 && selected != LevelManager.CurrentStage)
            {
                LevelManager.SelectStage(selected);
                Restart();
                return;
            }
        }

        CheckFall();
    }

    void Restart()
    {
        // Retries jump straight into gameplay - no title screen friction.
        if (GameSession.HasStarted)
        {
            LevelManager.RequestAutoStart();
        }

        Time.timeScale = 1f;
        EndBannerUI.Clear();
        LoadStageScene();
    }

    // Each stage lives in its own scene (Stage1..Stage5). Falls back to
    // reloading the current scene if the stage scene is not in the build.
    void LoadStageScene()
    {
        string sceneName = LevelManager.SceneNameForStage(LevelManager.CurrentStage);
        if (Application.CanStreamedLevelBeLoaded(sceneName))
        {
            SceneManager.LoadScene(sceneName);
        }
        else
        {
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
    }

    void CheckFall()
    {
        if (fellHandled || !GameSession.HasStarted || GameSession.HasEnded)
        {
            return;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (player == null || player.position.y > fallLimitY)
        {
            return;
        }

        RespawnSystem.KillPlayer(player.gameObject);
    }
}
