using UnityEngine;
using UnityEngine.InputSystem;

public class SpecialAttack : MonoBehaviour
{
    void Update()
    {
        if (!GameSession.CanControlPlayer || SpecialCutin.IsPlaying)
        {
            return;
        }

        Keyboard keyboard = Keyboard.current;
        if (keyboard == null || !keyboard.wKey.wasPressedThisFrame)
        {
            return;
        }

        if (!ScoreSystem.TryConsumeSpecial())
        {
            int remaining = ScoreSystem.SpecialChargeMax - ScoreSystem.SpecialCharge;
            JuiceManager.Popup(transform.position + Vector3.up * 1.6f,
                "あとコイン x" + remaining + " で ひっさつ!", new Color(1f, 0.84f, 0.25f), 0.9f);
            return;
        }

        SpecialCutin.Play(gameObject);
    }
}
