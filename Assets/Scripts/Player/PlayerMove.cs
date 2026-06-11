using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerMove : MonoBehaviour
{
    public float speed = 5f;
    public float ledgeAssistHeight = 0.75f;
    public float ledgeAssistForward = 0.28f;

    [System.NonSerialized] public float externalSpeedMultiplier = 1f;

    Rigidbody2D rb2d;
    Collider2D playerCollider;
    SpriteRenderer spriteRenderer;
    Animator animator;
    float inputX;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
    }

    void Update()
    {
        if (!GameSession.CanControlPlayer)
        {
            inputX = 0f;
            if (animator != null) animator.SetFloat("Speed", 0f);
            return;
        }

        float x = 0f;
        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            inputX = 0f;
            return;
        }

        if (keyboard.aKey.isPressed || keyboard.leftArrowKey.isPressed)
        {
            x = -1f;
        }

        if (keyboard.dKey.isPressed || keyboard.rightArrowKey.isPressed)
        {
            x = 1f;
        }

        if (spriteRenderer != null && x != 0f)
        {
            spriteRenderer.flipX = x < 0f;
        }

        if (animator != null)
        {
            animator.SetFloat("Speed", Mathf.Abs(x));
        }

        inputX = x;
    }

    void FixedUpdate()
    {
        float effectiveSpeed = speed * externalSpeedMultiplier * ScoreSystem.SpeedMultiplier;

        if (rb2d == null)
        {
            transform.position += new Vector3(inputX * effectiveSpeed * Time.fixedDeltaTime, 0, 0);
            return;
        }

        rb2d.linearVelocity = new Vector2(inputX * effectiveSpeed, rb2d.linearVelocity.y);
        TryAssistOntoPlatform();
    }

    void TryAssistOntoPlatform()
    {
        if (playerCollider == null || Mathf.Abs(inputX) < 0.01f)
        {
            return;
        }

        Bounds playerBounds = playerCollider.bounds;
        float direction = Mathf.Sign(inputX);
        float playerFront = direction > 0f ? playerBounds.max.x : playerBounds.min.x;

        BoxCollider2D[] platforms = FindObjectsByType<BoxCollider2D>();
        foreach (BoxCollider2D platform in platforms)
        {
            if (!platform.name.StartsWith("Platform_"))
            {
                continue;
            }

            Bounds platformBounds = platform.bounds;
            bool movingIntoPlatform = direction > 0f
                ? playerFront <= platformBounds.min.x + ledgeAssistForward && playerFront >= platformBounds.min.x - ledgeAssistForward
                : playerFront >= platformBounds.max.x - ledgeAssistForward && playerFront <= platformBounds.max.x + ledgeAssistForward;

            if (!movingIntoPlatform)
            {
                continue;
            }

            float distanceToTop = platformBounds.max.y - playerBounds.min.y;
            if (distanceToTop <= 0f || distanceToTop > ledgeAssistHeight)
            {
                continue;
            }

            transform.position += new Vector3(0f, distanceToTop + 0.03f, 0f);
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, Mathf.Max(0f, rb2d.linearVelocity.y));
            break;
        }
    }
}
