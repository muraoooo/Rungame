using UnityEngine;

// Blinks the player sprite during post-respawn invulnerability.
public class PlayerBlinker : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    bool wasBlinking;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        if (RespawnSystem.IsInvulnerable)
        {
            wasBlinking = true;
            bool visible = Mathf.Repeat(Time.time * 8f, 1f) < 0.6f;
            Color color = spriteRenderer.color;
            color.a = visible ? 1f : 0.3f;
            spriteRenderer.color = color;
        }
        else if (wasBlinking)
        {
            wasBlinking = false;
            Color color = spriteRenderer.color;
            color.a = 1f;
            spriteRenderer.color = color;
        }
    }
}
