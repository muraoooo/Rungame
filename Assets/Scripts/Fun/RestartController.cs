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
        Time.timeScale = 1f;
        EndBannerUI.Clear();
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
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

        fellHandled = true;
        RetroSfx.PlayGameOver();
        JuiceManager.Shake(0.4f);
        EndBannerUI.Show("UI/GameOverBanner");
        GameSession.EndGame(player.gameObject);
    }
}
