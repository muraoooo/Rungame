using UnityEngine;
using UnityEngine.InputSystem;

[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(Collider2D))]
public class PlayerJump : MonoBehaviour
{
    public float jumpPower = 8f;
    public float gravityScale = 2.15f;
    public float fallGravityMultiplier = 1.45f;
    public float jumpCutGravityMultiplier = 1.12f;
    public Sprite idleSprite;
    public Sprite jumpSprite;
    public float idleScale = 1.2f;
    public float jumpScale = 1.3f;
    public float acrobatScale = 1.2f;
    public bool fitColliderToBikeBody = true;
    public Vector2 fittedColliderSize = new Vector2(1.08f, 2.18f);
    public Vector2 fittedColliderOffset = new Vector2(0f, -0.28f);
    public Vector2 airborneColliderWorldSize = new Vector2(0.62f, 1.32f);
    public Vector2 airborneColliderWorldOffset = new Vector2(0f, 0.08f);
    public string[] groundObjectNamePrefixes = { "Ground", "Platform" };

    Rigidbody2D rb2d;
    Collider2D playerCollider;
    SpriteRenderer spriteRenderer;
    Animator animator;
    Vector3 baseScale;
    bool canAcrobat;
    bool isJumpVisual;
    bool isAcrobatVisual;
    bool useRideVisualAfterAcrobat;
    bool spaceJumpLocked;
    bool wasGroundedLastFrame;
    float lastGroundedTime = -10f;
    float jumpPressedTime = -10f;
    float lastVerticalVelocity;
    float jumpVisualStartTime;
    float acrobatVisualStartTime;
    float acrobatVisualEndTime;
    const float JumpGroundedIgnoreSeconds = 0.12f;
    const float SpaceJumpUnlockIgnoreSeconds = 0.24f;
    const float JumpRiseGravityGraceSeconds = 0.22f;
    const float JumpRiseGravityMultiplier = 0.78f;
    const float AcrobatVisualSeconds = 0.68f;
    const float AcrobatJumpMultiplier = 1.55f;
    const float AcrobatJumpGravityGraceSeconds = 0.38f;
    const float CoyoteSeconds = 0.1f;
    const float JumpBufferSeconds = 0.12f;
    const float GroundedTopTolerance = 0.32f;

    void Start()
    {
        rb2d = GetComponent<Rigidbody2D>();
        rb2d.constraints |= RigidbodyConstraints2D.FreezeRotation;
        // Dash + springs + boost rings reach speeds where discrete collision
        // tunnels through thin platforms; continuous detection prevents it.
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2d.gravityScale = gravityScale;
        jumpPower = Mathf.Max(jumpPower, 10f);
        playerCollider = GetComponent<Collider2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        baseScale = transform.localScale;

        // The bike sprite already has the right size in the scene. Scaling the
        // whole player up made the wheels look buried and confused grounding.
        idleScale = 1f;
        jumpScale = 1f;
        acrobatScale = 1f;
        fittedColliderSize = new Vector2(1.08f, 2.18f);
        fittedColliderOffset = new Vector2(0f, -0.28f);
        airborneColliderWorldSize = new Vector2(0.62f, 1.32f);
        airborneColliderWorldOffset = new Vector2(0f, 0.08f);
        FitColliderToBikeBodyIfNeeded();

        if (spriteRenderer != null && idleSprite == null)
        {
            idleSprite = spriteRenderer.sprite;
        }
    }

    void FitColliderToBikeBodyIfNeeded()
    {
        if (!fitColliderToBikeBody)
        {
            return;
        }

        BoxCollider2D boxCollider = playerCollider as BoxCollider2D;
        if (boxCollider == null)
        {
            return;
        }

        boxCollider.size = fittedColliderSize;
        boxCollider.offset = fittedColliderOffset;
    }

    void Update()
    {
        bool isGrounded = IsGrounded();

        if (isJumpVisual && Time.time < jumpVisualStartTime + JumpGroundedIgnoreSeconds)
        {
            isGrounded = false;
        }

        UpdateSpaceJumpLock(isGrounded);

        if (isGrounded)
        {
            lastGroundedTime = Time.time;
        }

        if (isGrounded && !wasGroundedLastFrame && lastVerticalVelocity < -3f)
        {
            JuiceManager.Dust(new Vector3(playerCollider.bounds.center.x, playerCollider.bounds.min.y, 0f), 7);
        }

        wasGroundedLastFrame = isGrounded;
        lastVerticalVelocity = rb2d.linearVelocity.y;

        if (animator != null && !isJumpVisual && !isAcrobatVisual && !useRideVisualAfterAcrobat)
        {
            animator.SetBool("IsGrounded", isGrounded);
        }

        if (!GameSession.CanControlPlayer)
        {
            UpdateJumpSprite();
            return;
        }

        Keyboard keyboard = Keyboard.current;

        if (keyboard == null)
        {
            UpdateJumpVisuals(isGrounded);
            return;
        }

        if (keyboard.spaceKey.wasPressedThisFrame)
        {
            jumpPressedTime = Time.time;
        }

        bool jumpBuffered = Time.time <= jumpPressedTime + JumpBufferSeconds;
        bool groundAvailable = isGrounded || Time.time <= lastGroundedTime + CoyoteSeconds;

        if (jumpBuffered && groundAvailable && !spaceJumpLocked)
        {
            jumpPressedTime = -10f;
            lastGroundedTime = -10f;
            RetroSfx.PlayJump();
            JuiceManager.Dust(new Vector3(playerCollider.bounds.center.x, playerCollider.bounds.min.y, 0f), 5);
            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, jumpPower);
            rb2d.gravityScale = gravityScale;
            spaceJumpLocked = true;
            if (animator != null)
            {
                isGrounded = false;
                animator.SetBool("IsGrounded", false);
                animator.ResetTrigger("Jump");
                animator.Play("Jump", 0, 0f);
            }
            canAcrobat = true;
            isJumpVisual = true;
            jumpVisualStartTime = Time.time;
        }

        // Acrobatics trigger
        if (keyboard.qKey.wasPressedThisFrame && !isGrounded && canAcrobat)
        {
            if (animator != null)
            {
                animator.SetBool("IsGrounded", false);
                animator.ResetTrigger("Jump");
                animator.ResetTrigger("Acrobat");
                animator.SetTrigger("Acrobat");
                animator.Play("Acrobat", 0, 0f);
            }
            float acrobatJumpSpeed = jumpPower * AcrobatJumpMultiplier;
            if (rb2d.linearVelocity.y < 0f)
            {
                acrobatJumpSpeed += Mathf.Abs(rb2d.linearVelocity.y) * 0.45f;
            }

            rb2d.linearVelocity = new Vector2(rb2d.linearVelocity.x, Mathf.Max(rb2d.linearVelocity.y + jumpPower * 0.45f, acrobatJumpSpeed));
            rb2d.gravityScale = gravityScale;
            ScoreSystem.AddTrick(transform.position);
            RetroSfx.PlayTrick();
            JuiceManager.Burst(transform.position, new Color(0.45f, 0.9f, 1f), 10, 4.5f);
            canAcrobat = false;
            isJumpVisual = false;
            isAcrobatVisual = true;
            useRideVisualAfterAcrobat = true;
            acrobatVisualStartTime = Time.time;
            acrobatVisualEndTime = Time.time + AcrobatVisualSeconds;
        }

        UpdateJumpGravity(keyboard, isGrounded);
        UpdateJumpVisuals(isGrounded);
    }

    void LateUpdate()
    {
        if (isAcrobatVisual || useRideVisualAfterAcrobat)
        {
            UpdateJumpVisuals(IsGrounded());
        }
    }

    void UpdateJumpGravity(Keyboard keyboard, bool isGrounded)
    {
        if (isJumpVisual && rb2d.linearVelocity.y > 0f && Time.time < jumpVisualStartTime + JumpRiseGravityGraceSeconds)
        {
            rb2d.gravityScale = gravityScale;
            return;
        }

        if (isAcrobatVisual && Time.time < acrobatVisualStartTime + AcrobatJumpGravityGraceSeconds)
        {
            rb2d.gravityScale = gravityScale;
            return;
        }

        if (isGrounded && rb2d.linearVelocity.y <= 0f)
        {
            rb2d.gravityScale = gravityScale;
            return;
        }

        if (rb2d.linearVelocity.y < 0f)
        {
            rb2d.gravityScale = gravityScale * fallGravityMultiplier;
            return;
        }

        if (rb2d.linearVelocity.y > 0f)
        {
            rb2d.gravityScale = gravityScale * (keyboard.spaceKey.isPressed ? JumpRiseGravityMultiplier : jumpCutGravityMultiplier);
            return;
        }

        rb2d.gravityScale = gravityScale;
    }

    void UpdateSpaceJumpLock(bool isGrounded)
    {
        if (!spaceJumpLocked)
        {
            return;
        }

        if (!isGrounded || rb2d.linearVelocity.y > 0.05f)
        {
            return;
        }

        if (Time.time <= jumpVisualStartTime + SpaceJumpUnlockIgnoreSeconds)
        {
            return;
        }

        spaceJumpLocked = false;
    }

    void UpdateJumpSprite()
    {
        UpdateJumpVisuals(IsGrounded());
    }

    void UpdateJumpVisuals(bool isGrounded)
    {
        if (isJumpVisual && Time.time < jumpVisualStartTime + JumpGroundedIgnoreSeconds)
        {
            isGrounded = false;
        }

        if (isGrounded && rb2d.linearVelocity.y <= 0.05f && Time.time > jumpVisualStartTime + JumpGroundedIgnoreSeconds)
        {
            isJumpVisual = false;
            canAcrobat = false;
            useRideVisualAfterAcrobat = false;
        }

        if (isGrounded || Time.time >= acrobatVisualEndTime)
        {
            isAcrobatVisual = false;
        }

        float visualScale = isJumpVisual || isAcrobatVisual ? jumpScale : idleScale;
        transform.localScale = new Vector3(baseScale.x * visualScale, baseScale.y * visualScale, baseScale.z);
        UpdateColliderFit(isGrounded, visualScale);

        // If animator exists, we let the animator handle visuals
        if (animator != null && animator.runtimeAnimatorController != null)
        {
            if (isAcrobatVisual)
            {
                animator.SetBool("IsGrounded", false);
                float acrobatProgress = Mathf.Clamp01((Time.time - acrobatVisualStartTime) / AcrobatVisualSeconds);
                animator.Play("Acrobat", 0, acrobatProgress);
                return;
            }

            if (isJumpVisual)
            {
                animator.SetBool("IsGrounded", false);
                animator.Play("Jump", 0, 0f);
                return;
            }

            if (useRideVisualAfterAcrobat)
            {
                animator.ResetTrigger("Jump");
                animator.SetBool("IsGrounded", true);
                string rideState = Mathf.Abs(animator.GetFloat("Speed")) > 0.1f ? "Run" : "Idle";
                animator.Play(rideState, 0, Mathf.Repeat(Time.time, 1f));
                return;
            }

            animator.SetBool("IsGrounded", isGrounded);
            return;
        }

        if (spriteRenderer == null || jumpSprite == null || idleSprite == null)
        {
            return;
        }

        spriteRenderer.sprite = isGrounded || useRideVisualAfterAcrobat ? idleSprite : jumpSprite;
    }

    // Keeps the world-space hitbox the same size even while the jump visual is
    // scaled up, and tucks it in while airborne so the player can clear enemies
    // that the sprite visually clears.
    void UpdateColliderFit(bool isGrounded, float visualScale)
    {
        if (!fitColliderToBikeBody)
        {
            return;
        }

        BoxCollider2D boxCollider = playerCollider as BoxCollider2D;
        if (boxCollider == null)
        {
            return;
        }

        if (!isGrounded)
        {
            float safeScale = Mathf.Max(0.01f, visualScale);
            boxCollider.size = airborneColliderWorldSize / safeScale;
            boxCollider.offset = airborneColliderWorldOffset / safeScale;
            return;
        }

        float compensate = idleScale / Mathf.Max(0.01f, visualScale);
        boxCollider.size = fittedColliderSize * compensate;
        boxCollider.offset = fittedColliderOffset * compensate;
    }

    bool IsGrounded()
    {
        Bounds bounds = playerCollider.bounds;
        Vector2 checkPosition = new Vector2(bounds.center.x, bounds.min.y - 0.05f);
        Vector2 checkSize = new Vector2(bounds.size.x * 0.8f, 0.1f);

        Collider2D[] hits = Physics2D.OverlapBoxAll(checkPosition, checkSize, 0f);

        foreach (Collider2D hit in hits)
        {
            if (hit == playerCollider || hit.isTrigger)
            {
                continue;
            }

            if (IsAllowedGroundCollider(hit) && IsUsableGroundTop(hit.bounds.max.y, bounds))
            {
                return true;
            }
        }

        return false;
    }

    bool IsUsableGroundTop(float groundTop, Bounds playerBounds)
    {
        if (groundTop <= playerBounds.min.y + GroundedTopTolerance)
        {
            return true;
        }

        return playerBounds.min.y <= groundTop && groundTop <= playerBounds.center.y - 0.08f;
    }

    bool IsAllowedGroundCollider(Collider2D hit)
    {
        if (hit == null)
        {
            return false;
        }

        string objectName = hit.gameObject.name;

        foreach (string prefix in groundObjectNamePrefixes)
        {
            if (!string.IsNullOrEmpty(prefix) && objectName.StartsWith(prefix))
            {
                return true;
            }
        }

        return false;
    }
}
