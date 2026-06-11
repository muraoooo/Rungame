using UnityEngine;

// Modern-Nintendo death rules: no game over screen. Dying respawns the player
// at the last checkpoint while the clock keeps running - lost time IS the
// penalty. Beginners keep playing, speedrunners aim for deathless runs.
public static class RespawnSystem
{
    public const int MaxHearts = 3;
    public static int Deaths { get; private set; }
    public static int CurrentHearts { get; private set; } = MaxHearts;
    public static bool IsInvulnerable => Time.time < invulnerableUntil;

    static Vector3 spawnPoint;
    static float invulnerableUntil;

    public static void Reset(Vector3 playerStart)
    {
        spawnPoint = playerStart;
        Deaths = 0;
        CurrentHearts = MaxHearts;
        invulnerableUntil = 0f;
    }

    public static void SetCheckpoint(Vector3 position)
    {
        spawnPoint = position;
    }

    public static void KillPlayer(GameObject player)
    {
        if (player == null || GameSession.HasEnded || !GameSession.HasStarted || IsInvulnerable)
        {
            return;
        }

        CurrentHearts = Mathf.Max(0, CurrentHearts - 1);
        invulnerableUntil = Time.time + 2f;
        ScoreSystem.BreakCombo();

        RetroSfx.PlayHurt();
        JuiceManager.Shake(0.4f);
        JuiceManager.HitStop(0.07f);
        JuiceManager.Burst(player.transform.position, new Color(1f, 0.35f, 0.3f), 16, 6f);

        Rigidbody2D body = player.GetComponent<Rigidbody2D>();
        if (CurrentHearts > 0)
        {
            if (body != null)
            {
                body.linearVelocity = new Vector2(body.linearVelocity.x * -0.25f, 7f);
            }

            JuiceManager.Popup(player.transform.position + Vector3.up * 1.6f, "ハート -" + 1, new Color(1f, 0.55f, 0.55f), 1f);
            return;
        }

        Deaths++;
        CurrentHearts = MaxHearts;
        player.transform.position = spawnPoint + Vector3.up * 0.5f;

        if (body != null)
        {
            body.linearVelocity = Vector2.zero;
        }

        JuiceManager.Popup(spawnPoint + Vector3.up * 1.6f, "ミス! ×" + Deaths, new Color(1f, 0.5f, 0.45f), 1.1f);
    }
}
