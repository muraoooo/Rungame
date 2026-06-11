using System.Collections;
using UnityEngine;

// Stage 1-3 mini-boss: a safe teacher for the final boss stomp rules.
public class MidBossSlime : MonoBehaviour
{
    const int MaxHealth = 2;
    const float ArenaMinX = 45f;
    const float ArenaMaxX = 51.2f;
    const float IntroTriggerX = 43f;
    const float HopSpeed = 2.2f;
    const float HopPower = 6.2f;
    const float LandingOpenSeconds = 1f;
    const float InvulnerableSeconds = 1.2f;

    GameObject barrier;
    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    Collider2D bossCollider;
    Sprite[] idleSprites;
    Transform player;
    Color baseColor;

    bool usesGeneratedKingSprite;
    bool introduced;
    bool defeated;
    bool wasAirborne;
    float invulnerableUntil;
    float nextHopTime;
    float nextImmunePopupTime;
    float frameTimer;
    int frame;
    int health;

    public static MidBossSlime Spawn(Vector3 position, GameObject barrier)
    {
        GameObject bossObject = new GameObject("MidBossSlime");
        bossObject.transform.position = position;
        bossObject.transform.localScale = Vector3.one * 2f;

        SpriteRenderer renderer = bossObject.AddComponent<SpriteRenderer>();
        Sprite kingSprite = Resources.Load<Sprite>("Enemies/KingSlime");
        if (kingSprite != null)
        {
            renderer.sprite = kingSprite;
            renderer.color = new Color(0.52f, 1f, 0.5f, 1f);
            if (kingSprite.bounds.size.y > 0.01f)
            {
                bossObject.transform.localScale = Vector3.one * (2f / kingSprite.bounds.size.y);
            }
        }
        else
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Slime/Idle");
            if (sprites != null && sprites.Length > 0)
            {
                renderer.sprite = sprites[0];
            }
            renderer.color = new Color(0.55f, 1f, 0.5f, 1f);
        }
        renderer.sortingOrder = 12;

        BoxCollider2D collider = bossObject.AddComponent<BoxCollider2D>();
        if (renderer.sprite != null)
        {
            Bounds spriteBounds = renderer.sprite.bounds;
            collider.size = new Vector2(spriteBounds.size.x * 0.82f, spriteBounds.size.y * 0.55f);
            collider.offset = new Vector2(spriteBounds.center.x, spriteBounds.center.y - spriteBounds.extents.y * 0.22f);
        }

        Rigidbody2D body = bossObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.gravityScale = 3.2f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        MidBossSlime boss = bossObject.AddComponent<MidBossSlime>();
        boss.barrier = barrier;
        return boss;
    }

    void Start()
    {
        health = MaxHealth;
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        bossCollider = GetComponent<Collider2D>();
        baseColor = spriteRenderer != null ? spriteRenderer.color : Color.white;
        usesGeneratedKingSprite = spriteRenderer != null && spriteRenderer.sprite != null && spriteRenderer.sprite.name == "KingSlime";
        if (!usesGeneratedKingSprite)
        {
            idleSprites = Resources.LoadAll<Sprite>("Slime/Idle");
        }
        nextHopTime = Time.time + 1f;
    }

    void Update()
    {
        AnimateSprite();
        UpdateInvulnerabilityFlash();
        CheckIntro();
    }

    void CheckIntro()
    {
        if (introduced || defeated || !GameSession.HasStarted || GameSession.HasEnded)
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

        if (player == null || player.position.x < IntroTriggerX)
        {
            return;
        }

        introduced = true;
        nextHopTime = Time.time + 0.8f;
        RetroSfx.PlayRoar();
        JuiceManager.Shake(0.28f);
        JuiceManager.Popup(transform.position + Vector3.up * 2.2f, "VS ビッグスライム!", new Color(0.55f, 1f, 0.5f), 1.4f);
    }

    void AnimateSprite()
    {
        if (defeated || usesGeneratedKingSprite || idleSprites == null || idleSprites.Length == 0 || spriteRenderer == null)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        if (frameTimer >= 0.12f)
        {
            frameTimer = 0f;
            frame = (frame + 1) % idleSprites.Length;
            spriteRenderer.sprite = idleSprites[frame];
        }
    }

    void UpdateInvulnerabilityFlash()
    {
        if (spriteRenderer == null || defeated)
        {
            return;
        }

        if (Time.time < invulnerableUntil)
        {
            bool flashOn = Mathf.Repeat(Time.unscaledTime * 10f, 1f) < 0.5f;
            spriteRenderer.color = flashOn ? Color.white : baseColor;
        }
        else
        {
            float damage = (float)(MaxHealth - health) / MaxHealth;
            spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.88f, 0.35f, 1f), damage);
        }
    }

    void FixedUpdate()
    {
        if (defeated || !introduced || !GameSession.HasStarted || GameSession.HasEnded)
        {
            return;
        }

        bool grounded = Mathf.Abs(body.linearVelocity.y) < 0.2f;
        if (wasAirborne && grounded)
        {
            nextHopTime = Time.time + LandingOpenSeconds;
            JuiceManager.Dust(transform.position + Vector3.down * 0.65f, 7);
            JuiceManager.Shake(0.12f);
        }
        wasAirborne = !grounded;

        if (grounded && Time.time >= nextHopTime && player != null)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            if (direction == 0f)
            {
                direction = -1f;
            }

            body.linearVelocity = new Vector2(direction * HopSpeed, HopPower);
            nextHopTime = Time.time + 2f;
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction < 0f;
            }
        }

        Vector2 position = body.position;
        if (position.x < ArenaMinX || position.x > ArenaMaxX)
        {
            position.x = Mathf.Clamp(position.x, ArenaMinX, ArenaMaxX);
            body.position = position;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandlePlayerContact(collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandlePlayerContact(collision);
    }

    void HandlePlayerContact(Collision2D collision)
    {
        if (defeated || GameSession.HasEnded || !collision.gameObject.CompareTag("Player"))
        {
            return;
        }

        GameObject playerObject = collision.gameObject;
        if (IsStomp(playerObject))
        {
            TakeHit(playerObject);
        }
        else
        {
            RespawnSystem.KillPlayer(playerObject, "ぶつかった!");
        }
    }

    bool IsStomp(GameObject playerObject)
    {
        Rigidbody2D playerBody = playerObject.GetComponent<Rigidbody2D>();
        Collider2D playerCollider = playerObject.GetComponent<Collider2D>();
        if (playerBody == null || playerCollider == null || bossCollider == null)
        {
            return false;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds bossBounds = bossCollider.bounds;
        bool overlaps = playerBounds.max.x > bossBounds.min.x + 0.1f && playerBounds.min.x < bossBounds.max.x - 0.1f;
        bool feetNearTop = playerBounds.min.y >= bossBounds.max.y - 0.45f;
        bool above = playerBounds.center.y >= bossBounds.center.y + bossBounds.extents.y * 0.35f;
        bool falling = playerBody.linearVelocity.y <= 0.35f;
        return overlaps && feetNearTop && above && falling;
    }

    void TakeHit(GameObject playerObject)
    {
        Rigidbody2D playerBody = playerObject.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, 10.5f);
        }

        if (Time.time < invulnerableUntil)
        {
            return;
        }

        health--;
        invulnerableUntil = Time.time + InvulnerableSeconds;
        RetroSfx.PlayStomp();
        JuiceManager.Shake(0.35f);
        JuiceManager.HitStop(0.08f);
        JuiceManager.Burst(transform.position + Vector3.up, new Color(0.6f, 1f, 0.55f), 14, 6f);

        if (health <= 0)
        {
            Defeat();
        }
        else
        {
            JuiceManager.Popup(transform.position + Vector3.up * 2f, "あと " + health + " かい!", Color.white, 1.2f);
        }
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.GetComponent<SlimeDamagingAttack>() == null)
        {
            return;
        }

        Destroy(other.gameObject);
        if (Time.time >= nextImmunePopupTime)
        {
            nextImmunePopupTime = Time.time + 1f;
            JuiceManager.Popup(transform.position + Vector3.up * 1.8f, "きかない!", new Color(0.8f, 0.8f, 0.9f), 1f);
        }
    }

    void Defeat()
    {
        defeated = true;
        if (bossCollider != null)
        {
            bossCollider.enabled = false;
        }

        RetroSfx.PlaySpecialBoom();
        JuiceManager.Shake(0.5f);
        JuiceManager.HitStop(0.14f);
        JuiceManager.Confetti(transform.position + Vector3.up * 1.5f, 38);
        JuiceManager.Popup(transform.position + Vector3.up * 2.4f, "ビッグスライム げきは!", new Color(1f, 0.9f, 0.3f), 1.6f);
        ScoreSystem.AddBonus(1000, transform.position + Vector3.up * 1.4f, "MID BOSS");

        if (barrier != null)
        {
            JuiceManager.Burst(barrier.transform.position, new Color(0.55f, 1f, 0.6f), 20, 6f);
            Destroy(barrier);
        }

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        const float duration = 0.85f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float wobble = 1f + Mathf.Sin(elapsed * 34f) * 0.1f * (1f - progress);
            transform.localScale = new Vector3(
                startScale.x * wobble * (1f - progress * 0.5f),
                startScale.y * (1f - progress) * wobble,
                startScale.z);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), progress * progress);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
