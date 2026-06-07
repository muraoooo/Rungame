using UnityEngine;

public static class GameSession
{
    public static bool CanControlPlayer { get; private set; } = true;
    public static bool HasEnded { get; private set; }
    public static bool HasStarted { get; private set; }
    public static bool ReachedGoal { get; private set; }

    static float startTime;
    static float endTime;

    public static float ElapsedTime
    {
        get
        {
            if (!HasStarted)
            {
                return 0f;
            }

            if (HasEnded)
            {
                return Mathf.Max(0f, endTime - startTime);
            }

            return Mathf.Max(0f, Time.realtimeSinceStartup - startTime);
        }
    }

    public static void WaitForStart()
    {
        HasEnded = false;
        HasStarted = false;
        ReachedGoal = false;
        CanControlPlayer = false;
        startTime = 0f;
        endTime = 0f;
        Time.timeScale = 0f;
    }

    public static void StartGame()
    {
        HasEnded = false;
        HasStarted = true;
        ReachedGoal = false;
        CanControlPlayer = true;
        startTime = Time.realtimeSinceStartup;
        endTime = startTime;
        Time.timeScale = 1f;
    }

    public static void EndGame(GameObject player, bool reachedGoal = false)
    {
        if (!HasEnded)
        {
            endTime = Time.realtimeSinceStartup;
        }

        HasEnded = true;
        ReachedGoal = reachedGoal;
        CanControlPlayer = false;
        StopPlayer(player);
        Time.timeScale = 0f;
    }

    public static void StopPlayer(GameObject player)
    {
        if (player == null)
        {
            return;
        }

        PlayerMove playerMove = player.GetComponent<PlayerMove>();
        if (playerMove != null)
        {
            playerMove.enabled = false;
        }

        PlayerJump playerJump = player.GetComponent<PlayerJump>();
        if (playerJump != null)
        {
            playerJump.enabled = false;
        }

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();
        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }
}
