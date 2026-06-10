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
            Restart();
            return;
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
