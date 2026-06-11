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
            return;
        }

        SpecialCutin.Play(gameObject);
    }
}
