using UnityEngine;
using UnityEngine.UI;
using System;
using System.Collections;

[RequireComponent(typeof(Collider2D))]
public class GameOver : MonoBehaviour
{
    public GameObject gameOverText;
    public float stompBouncePower = 8f;
    public float stompCheckMargin = 0.5f;
    public bool fitColliderToSlimeBody = true;
    public Sprite flattenedSprite;
    public float defeatedVisibleSeconds = 0.35f;
    public float patrolDistance = 2f;
    public float patrolSpeed = 1.5f;
    public float moveSecondsBeforeIdle = 2.2f;
    public float idlePauseSeconds = 0.8f;
    public float gravityScale = 3.2f;
    public bool snapToGroundOnStart = true;
    public float groundSnapDistance = 20f;
    public float groundVisualOffset = 0.28f;
    public int spriteSortingOrder = 8;
    public Sprite[] idleSprites;
    public Sprite[] damageSprites;
    public float idleFrameSeconds = 0.12f;
    public float damageFrameSeconds = 0.07f;
    public Vector2 damageKnockback = new Vector2(6.5f, 3f);
    public string[] damagingTags = { "PlayerAttack", "Attack", "PlayerBullet", "Bullet" };
    public bool destroyAttackOnHit = true;

    bool isGameOver;
    bool isDefeated;
    bool showedImageBanner;
    bool isPatrolPaused;
    Vector3 startPosition;
    Rigidbody2D rb2d;
    Collider2D enemyCollider;
    SpriteRenderer spriteRenderer;
    int moveDirection = 1;
    float patrolStateTimer;
    Coroutine spriteAnimationCoroutine;
    const float MinimumStompMargin = 0.22f;
    const float StompHorizontalInsetRatio = 0.12f;
    const float EnemyUpperBodyRatio = 0.45f;
    const float LandingVelocityTolerance = 0.35f;

    void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
        rb2d = GetComponent<Rigidbody2D>();

        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }

        rb2d.bodyType = RigidbodyType2D.Dynamic;
        rb2d.gravityScale = gravityScale;
        rb2d.constraints |= RigidbodyConstraints2D.FreezeRotation;
        rb2d.interpolation = RigidbodyInterpolation2D.Interpolate;

        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            spriteRenderer.sortingOrder = Mathf.Max(spriteRenderer.sortingOrder, spriteSortingOrder);
        }

        LoadSlimeSpritesIfNeeded();
        FitColliderToSpriteBodyIfNeeded();
        StartIdleAnimation();

        if (snapToGroundOnStart)
        {
            SnapToGround();
        }

        startPosition = transform.position;

        if (gameOverText != null)
        {
            gameOverText.SetActive(false);
        }
    }

    void FixedUpdate()
    {
        if (isDefeated || patrolDistance <= 0f || patrolSpeed <= 0f)
        {
            return;
        }

        patrolStateTimer += Time.fixedDeltaTime;

        if (isPatrolPaused)
        {
            rb2d.linearVelocity = new Vector2(0f, rb2d.linearVelocity.y);

            if (patrolStateTimer >= idlePauseSeconds)
            {
                patrolStateTimer = 0f;
                isPatrolPaused = false;
            }

            return;
        }

        if (moveSecondsBeforeIdle > 0f && patrolStateTimer >= moveSecondsBeforeIdle)
        {
            patrolStateTimer = 0f;
            isPatrolPaused = true;
            return;
        }

        float previousX = rb2d.position.x;
        float nextX = rb2d.position.x + moveDirection * patrolSpeed * Time.fixedDeltaTime;

        if (Mathf.Abs(nextX - startPosition.x) > patrolDistance)
        {
            moveDirection *= -1;
            nextX = Mathf.Clamp(nextX, startPosition.x - patrolDistance, startPosition.x + patrolDistance);
        }

        Vector2 nextPosition = new Vector2(nextX, rb2d.position.y);
        rb2d.MovePosition(nextPosition);

        float moveX = nextPosition.x - previousX;
        if (spriteRenderer != null && Mathf.Abs(moveX) > 0.001f)
        {
            spriteRenderer.flipX = moveX < 0f;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        if (isDefeated)
        {
            return;
        }

        GameObject other = collision.gameObject;

        if (TryHandleAttackHit(other, collision.transform.position))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other, collision);
            return;
        }

        ReverseDirectionIfBlocked(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        if (isDefeated)
        {
            return;
        }

        GameObject other = collision.gameObject;

        if (TryHandleAttackHit(other, collision.transform.position))
        {
            return;
        }

        if (other.CompareTag("Player"))
        {
            HandlePlayerCollision(other, collision);
            return;
        }

        ReverseDirectionIfBlocked(collision);
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (isDefeated)
        {
            return;
        }

        GameObject other = collision.gameObject;

        if (TryHandleAttackHit(other, collision.transform.position))
        {
            return;
        }

        if (other.CompareTag("Player") && IsPlayerInDamageZone(collision))
        {
            GameOverPlayer(other);
        }
    }

    void ReverseDirectionIfBlocked(Collision2D collision)
    {
        foreach (ContactPoint2D contact in collision.contacts)
        {
            if (Mathf.Abs(contact.normal.x) < 0.5f)
            {
                continue;
            }

            if (Mathf.Sign(contact.normal.x) == -moveDirection)
            {
                moveDirection *= -1;
                return;
            }
        }
    }

    void HandlePlayerCollision(GameObject player, Collision2D collision)
    {
        if (IsPlayerStomping(player, collision))
        {
            DefeatEnemy(player, player.transform.position);
            return;
        }

        // Any non-stomp contact hurts. The old "damage zone" compared the
        // enemy's head to the player's CENTER, so side contact with a short
        // slime did nothing at all.
        GameOverPlayer(player);
    }

    bool TryHandleAttackHit(GameObject target, Vector3 hitSourcePosition)
    {
        if (!IsDamagingObject(target))
        {
            return false;
        }

        DefeatEnemy(null, hitSourcePosition);
        DestroyAttackObject(target);
        return true;
    }

    // Honest Mario rule: a stomp means the player's FEET are at or above the
    // enemy's head while falling. The old check used the player's center,
    // which is always above a short slime, so walking into one sideways
    // counted as a stomp and the player never got hurt.
    bool IsPlayerStomping(GameObject player, Collision2D collision)
    {
        Rigidbody2D playerRigidbody = player.GetComponent<Rigidbody2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();

        if (playerRigidbody == null || enemyCollider == null || playerCollider == null)
        {
            return false;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;
        float margin = EffectiveStompMargin();
        float horizontalInset = Mathf.Min(enemyBounds.extents.x * StompHorizontalInsetRatio, 0.12f);
        bool playerOverlapsEnemyTop = playerBounds.max.x > enemyBounds.min.x + horizontalInset
            && playerBounds.min.x < enemyBounds.max.x - horizontalInset;
        bool playerFeetAreNearEnemyTop = playerBounds.min.y >= enemyBounds.max.y - margin;
        bool playerIsFallingOrLanding = playerRigidbody.linearVelocity.y <= LandingVelocityTolerance;

        return playerOverlapsEnemyTop && playerFeetAreNearEnemyTop && playerIsFallingOrLanding;
    }

    bool IsPlayerInDamageZone(Collider2D playerCollider)
    {
        if (playerCollider == null || enemyCollider == null)
        {
            return false;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;
        float margin = EffectiveStompMargin();
        bool enemyCanReachPlayerBody = enemyBounds.max.y + margin >= playerBounds.center.y;

        return enemyCanReachPlayerBody;
    }

    bool HasUpperBodyContact(Collision2D collision, Bounds enemyBounds, float margin)
    {
        float upperBodyLine = EnemyUpperBodyLine(enemyBounds);

        foreach (ContactPoint2D contact in collision.contacts)
        {
            bool contactIsHighEnough = contact.point.y >= upperBodyLine - margin;
            bool contactIsWithinEnemyWidth = contact.point.x >= enemyBounds.min.x - margin
                && contact.point.x <= enemyBounds.max.x + margin;

            if (contactIsHighEnough && contactIsWithinEnemyWidth)
            {
                return true;
            }
        }

        return false;
    }

    float EnemyUpperBodyLine(Bounds enemyBounds)
    {
        return Mathf.Lerp(enemyBounds.min.y, enemyBounds.max.y, EnemyUpperBodyRatio);
    }

    float EffectiveStompMargin()
    {
        return Mathf.Max(stompCheckMargin, MinimumStompMargin);
    }

    public void DefeatBySpecial(Vector3 sourcePosition)
    {
        DefeatEnemy(null, sourcePosition);
    }

    void DefeatEnemy(GameObject player, Vector3 hitSourcePosition)
    {
        if (isDefeated)
        {
            return;
        }

        isDefeated = true;

        ScoreSystem.RegisterStomp(transform.position);
        RetroSfx.PlayStomp();
        JuiceManager.Shake(0.3f);
        JuiceManager.HitStop(0.06f);
        JuiceManager.Burst(transform.position, new Color(0.5f, 0.9f, 0.45f), 14, 5.5f);

        Rigidbody2D playerRigidbody = player != null ? player.GetComponent<Rigidbody2D>() : null;
        if (playerRigidbody != null)
        {
            playerRigidbody.linearVelocity = new Vector2(playerRigidbody.linearVelocity.x, stompBouncePower);
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StopSpriteAnimation();
        StartCoroutine(DestroyAfterDefeat(hitSourcePosition));
    }

    IEnumerator DestroyAfterDefeat(Vector3 hitSourcePosition)
    {
        float elapsed = 0f;
        Vector3 defeatedScale = transform.localScale;
        Vector2 awayFromHit = transform.position - hitSourcePosition;
        float knockbackDirection = awayFromHit.x >= 0f ? 1f : -1f;

        if (Mathf.Abs(awayFromHit.x) < 0.05f)
        {
            knockbackDirection = -moveDirection;
        }

        rb2d.linearVelocity = new Vector2(damageKnockback.x * knockbackDirection, damageKnockback.y);

        if (spriteRenderer != null && flattenedSprite != null)
        {
            spriteRenderer.sprite = flattenedSprite;
        }
        else if (spriteRenderer != null && damageSprites != null && damageSprites.Length > 0)
        {
            spriteRenderer.sprite = damageSprites[damageSprites.Length - 1];
        }
        else
        {
            transform.localScale = new Vector3(transform.localScale.x * 1.25f, transform.localScale.y * 0.35f, transform.localScale.z);
        }

        while (elapsed < defeatedVisibleSeconds)
        {
            float wobble = Mathf.Sin(elapsed * 50f) * 0.08f;
            transform.localScale = new Vector3(
                defeatedScale.x * (1f + wobble),
                defeatedScale.y * (1f - Mathf.Abs(wobble) * 0.5f),
                defeatedScale.z
            );

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    void LoadSlimeSpritesIfNeeded()
    {
        if (idleSprites == null || idleSprites.Length == 0)
        {
            idleSprites = Resources.LoadAll<Sprite>("Slime/Idle");
            Array.Sort(idleSprites, (left, right) => string.CompareOrdinal(left.name, right.name));
        }

        if (damageSprites == null || damageSprites.Length == 0)
        {
            damageSprites = Resources.LoadAll<Sprite>("Slime/Damage");
            Array.Sort(damageSprites, (left, right) => string.CompareOrdinal(left.name, right.name));
        }
    }

    void FitColliderToSpriteBodyIfNeeded()
    {
        if (!fitColliderToSlimeBody || spriteRenderer == null || spriteRenderer.sprite == null)
        {
            return;
        }

        BoxCollider2D boxCollider = enemyCollider as BoxCollider2D;
        if (boxCollider == null)
        {
            return;
        }

        Bounds spriteBounds = spriteRenderer.sprite.bounds;
        boxCollider.size = new Vector2(spriteBounds.size.x * 0.78f, spriteBounds.size.y * 0.5f);
        boxCollider.offset = new Vector2(spriteBounds.center.x, spriteBounds.center.y - spriteBounds.extents.y * 0.26f);
    }

    void StartIdleAnimation()
    {
        if (idleSprites == null || idleSprites.Length == 0 || spriteRenderer == null)
        {
            return;
        }

        StopSpriteAnimation();
        spriteAnimationCoroutine = StartCoroutine(PlayIdleAnimation());
    }

    void StopSpriteAnimation()
    {
        if (spriteAnimationCoroutine == null)
        {
            return;
        }

        StopCoroutine(spriteAnimationCoroutine);
        spriteAnimationCoroutine = null;
    }

    IEnumerator PlayIdleAnimation()
    {
        int frame = 0;

        while (!isDefeated)
        {
            spriteRenderer.sprite = idleSprites[frame];
            frame = (frame + 1) % idleSprites.Length;
            yield return new WaitForSeconds(idleFrameSeconds);
        }
    }

    bool IsDamagingObject(GameObject target)
    {
        if (target == null)
        {
            return false;
        }

        if (GetDamagingAttack(target) != null)
        {
            return true;
        }

        if (damagingTags == null)
        {
            return false;
        }

        foreach (string damageTag in damagingTags)
        {
            if (!string.IsNullOrWhiteSpace(damageTag) && HasTag(target, damageTag))
            {
                return true;
            }
        }

        return false;
    }

    bool HasTag(GameObject target, string tagName)
    {
        if (target == null || string.IsNullOrEmpty(tagName)) return false;
        return target.tag == tagName;
    }

    void DestroyAttackObject(GameObject attackObject)
    {
        if (!destroyAttackOnHit || attackObject == null)
        {
            return;
        }

        SlimeDamagingAttack attack = GetDamagingAttack(attackObject);
        Destroy(attack != null ? attack.gameObject : attackObject);
    }

    SlimeDamagingAttack GetDamagingAttack(GameObject target)
    {
        if (target == null)
        {
            return null;
        }

        SlimeDamagingAttack attack = target.GetComponent<SlimeDamagingAttack>();
        if (attack != null)
        {
            return attack;
        }

        return target.GetComponentInParent<SlimeDamagingAttack>();
    }

    void GameOverPlayer(GameObject player)
    {
        if (isDefeated || !player.CompareTag("Player"))
        {
            return;
        }

        // Star power: during FEVER, touching an enemy destroys it instead.
        if (ScoreSystem.IsFever)
        {
            DefeatEnemy(player, player.transform.position);
            return;
        }

        // No game-over screen: respawn at the last checkpoint.
        RespawnSystem.KillPlayer(player);
        return;
    }

    void SnapToGround()
    {
        if (enemyCollider == null)
        {
            return;
        }

        Bounds bounds = enemyCollider.bounds;
        Vector2 rayStart = new Vector2(bounds.center.x, bounds.max.y + 0.1f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, Vector2.down, groundSnapDistance + bounds.size.y + 0.1f);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == enemyCollider || hit.collider.isTrigger || hit.normal.y < 0.5f)
            {
                continue;
            }

            float yOffset = hit.point.y - bounds.min.y + groundVisualOffset;
            transform.position += new Vector3(0f, yOffset, 0f);
            return;
        }
    }

    void OnGUI()
    {
        if (!isGameOver || gameOverText != null || showedImageBanner)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 64;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.red;

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "ゲームオーバーだよ", style);
    }
}
