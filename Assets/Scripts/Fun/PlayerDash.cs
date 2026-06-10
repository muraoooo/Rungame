using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerDash : MonoBehaviour
{
    public float dashMultiplier = 2.3f;
    public float dashSeconds = 0.24f;
    public float cooldownSeconds = 1.1f;

    PlayerMove playerMove;
    SpriteRenderer spriteRenderer;
    float dashEndTime = -10f;
    float nextDashTime = -10f;
    float nextGhostTime;

    public void RechargeNow()
    {
        nextDashTime = -10f;
    }

    void Start()
    {
        playerMove = GetComponent<PlayerMove>();
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        bool dashing = Time.time < dashEndTime;

        if (GameSession.CanControlPlayer)
        {
            Keyboard keyboard = Keyboard.current;
            bool dashPressed = keyboard != null
                && (keyboard.leftShiftKey.wasPressedThisFrame
                    || keyboard.rightShiftKey.wasPressedThisFrame
                    || keyboard.eKey.wasPressedThisFrame);

            if (dashPressed && Time.time >= nextDashTime)
            {
                dashEndTime = Time.time + dashSeconds;
                nextDashTime = Time.time + cooldownSeconds;
                dashing = true;
                RetroSfx.PlayDash();
                JuiceManager.Shake(0.15f);
            }
        }

        if (playerMove != null)
        {
            playerMove.externalSpeedMultiplier = dashing ? dashMultiplier : 1f;
        }

        bool leavingTrail = dashing || ScoreSystem.IsFever;
        if (leavingTrail && Time.unscaledTime >= nextGhostTime && spriteRenderer != null)
        {
            nextGhostTime = Time.unscaledTime + (dashing ? 0.035f : 0.07f);
            JuiceManager.SpawnGhost(spriteRenderer, transform, ScoreSystem.IsFever);
        }
    }
}
