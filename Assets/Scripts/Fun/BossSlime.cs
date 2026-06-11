using System.Collections;
using UnityEngine;

// King Slime boss for stage 1-5. Bowser rules: only stomps hurt it (3 hits),
// it gets faster and angrier with each hit, spits poison globs, and guards
// the goal behind a barrier until defeated.
public class BossSlime : MonoBehaviour
{
    public static BossSlime Instance { get; private set; }
    public static bool BossFightActive =>
        Instance != null && Instance.introduced && !Instance.defeated;

    public int maxHealth = 3;
    public int Health { get; private set; }

    const float ArenaMinX = 40f;
    const float ArenaMaxX = 50.2f;
    const float IntroTriggerX = 36f;
    const float InvulnerableSeconds = 1.6f;
    const float SpitTelegraphSeconds = 0.4f;

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
    float nextSpitTime;
    float nextImmunePopupTime;
    float spitTelegraphUntil;
    float frameTimer;
    int frame;
    bool spitTelegraphing;

    public static BossSlime Spawn(Vector3 position, GameObject barrier)
    {
        GameObject bossObject = new GameObject("BossSlime");
        bossObject.transform.position = position;
        bossObject.transform.localScale = Vector3.one * 3.2f;

        SpriteRenderer renderer = bossObject.AddComponent<SpriteRenderer>();
        Sprite kingSprite = Resources.Load<Sprite>("Enemies/KingSlime");
        if (kingSprite != null)
        {
            renderer.sprite = kingSprite;
            renderer.color = Color.white;
            // The generated sprite imports at PPU 1024 (~0.5 units tall);
            // scale to a proper boss presence of ~3 units regardless of PPU.
            if (kingSprite.bounds.size.y > 0.01f)
            {
                bossObject.transform.localScale = Vector3.one * (3.35f / kingSprite.bounds.size.y);
            }
        }
        else
        {
            Sprite[] sprites = Resources.LoadAll<Sprite>("Slime/Idle");
            if (sprites != null && sprites.Length > 0)
            {
                renderer.sprite = sprites[0];
            }
            renderer.color = new Color(0.85f, 0.5f, 1f, 1f);
        }
        renderer.sortingOrder = 12;

        BoxCollider2D collider = bossObject.AddComponent<BoxCollider2D>();
        if (renderer.sprite != null)
        {
            Bounds spriteBounds = renderer.sprite.bounds;
            collider.size = new Vector2(spriteBounds.size.x * 0.8f, spriteBounds.size.y * 0.85f);
            collider.offset = new Vector2(spriteBounds.center.x, spriteBounds.center.y - spriteBounds.extents.y * 0.03f);
        }

        Rigidbody2D body = bossObject.AddComponent<Rigidbody2D>();
        body.bodyType = RigidbodyType2D.Dynamic;
        body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        body.gravityScale = 3.2f;
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;

        BossSlime boss = bossObject.AddComponent<BossSlime>();
        boss.barrier = barrier;
        return boss;
    }

    void Start()
    {
        Instance = this;
        Health = maxHealth;
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
        nextSpitTime = Time.time + 2.5f;
    }

    void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
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
        nextSpitTime = Time.time + 1.6f;
        RetroSfx.PlayRoar();
        JuiceManager.Shake(0.45f);
        JuiceManager.Popup(transform.position + Vector3.up * 2.6f, "VS キングスライム!!", new Color(1f, 0.4f, 0.9f), 1.6f);
    }

    void AnimateSprite()
    {
        if (defeated || usesGeneratedKingSprite || idleSprites == null || idleSprites.Length == 0 || spriteRenderer == null)
        {
            return;
        }

        frameTimer += Time.deltaTime;
        float frameSeconds = introduced ? 0.08f : 0.14f;
        if (frameTimer >= frameSeconds)
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

        if (Time.time < spitTelegraphUntil)
        {
            float flash = Mathf.Repeat(Time.unscaledTime * 14f, 1f) < 0.68f ? 1f : 0.82f;
            spriteRenderer.color = Color.Lerp(baseColor, Color.white, flash);
        }
        else if (Time.time < invulnerableUntil)
        {
            bool flashOn = Mathf.Repeat(Time.unscaledTime * 10f, 1f) < 0.5f;
            spriteRenderer.color = flashOn ? Color.white : baseColor;
        }
        else
        {
            // Angrier = redder
            float rage = (float)(maxHealth - Health) / maxHealth;
            spriteRenderer.color = Color.Lerp(baseColor, new Color(1f, 0.3f, 0.35f, 1f), rage);
        }
    }

    void FixedUpdate()
    {
        if (defeated || !introduced || !GameSession.HasStarted || GameSession.HasEnded)
        {
            return;
        }

        int rage = maxHealth - Health;
        bool grounded = Mathf.Abs(body.linearVelocity.y) < 0.2f;

        if (wasAirborne && grounded)
        {
            JuiceManager.Dust(transform.position + Vector3.down * 0.8f, 10);
            JuiceManager.Shake(0.18f);
        }
        wasAirborne = !grounded;

        // Hop toward the player, faster while enraged
        if (!spitTelegraphing && grounded && Time.time >= nextHopTime && player != null)
        {
            float direction = Mathf.Sign(player.position.x - transform.position.x);
            float hopSpeed = 2.6f + rage * 0.9f;
            body.linearVelocity = new Vector2(direction * hopSpeed, 6.8f + rage * 0.5f);
            nextHopTime = Time.time + Mathf.Max(0.7f, 1.7f - rage * 0.35f);
            if (spriteRenderer != null)
            {
                spriteRenderer.flipX = direction < 0f;
            }
        }

        // Spit poison globs in an arc
        if (!spitTelegraphing && Time.time >= nextSpitTime && player != null)
        {
            StartCoroutine(TelegraphThenSpit(rage));
        }

        // Keep the boss inside its arena
        Vector2 position = body.position;
        if (position.x < ArenaMinX || position.x > ArenaMaxX)
        {
            position.x = Mathf.Clamp(position.x, ArenaMinX, ArenaMaxX);
            body.position = position;
            body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
        }
    }

    IEnumerator TelegraphThenSpit(int rage)
    {
        spitTelegraphing = true;
        spitTelegraphUntil = Time.time + SpitTelegraphSeconds;

        Vector3 baseScale = transform.localScale;
        Vector3 mouth = transform.position + Vector3.up * 1f;
        Vector2[] velocities = CreateSpitVelocities(rage);
        ShowLandingWarnings(mouth, velocities);

        float elapsed = 0f;
        while (elapsed < SpitTelegraphSeconds)
        {
            if (defeated || GameSession.HasEnded)
            {
                transform.localScale = baseScale;
                spitTelegraphing = false;
                spitTelegraphUntil = 0f;
                yield break;
            }

            float progress = elapsed / SpitTelegraphSeconds;
            float swell = 1f + Mathf.Sin(progress * Mathf.PI) * 0.1f;
            transform.localScale = new Vector3(baseScale.x * swell, baseScale.y * (1f + (swell - 1f) * 0.7f), baseScale.z);

            elapsed += Time.deltaTime;
            yield return null;
        }

        transform.localScale = baseScale;
        spitTelegraphing = false;
        spitTelegraphUntil = 0f;

        if (!defeated && GameSession.HasStarted && !GameSession.HasEnded)
        {
            SpitGlobs(mouth, velocities);
        }

        nextSpitTime = Time.time + Mathf.Max(1.4f, 3.2f - rage * 0.7f);
    }

    Vector2[] CreateSpitVelocities(int rage)
    {
        float direction = player != null ? Mathf.Sign(player.position.x - transform.position.x) : -1f;
        if (direction == 0f)
        {
            direction = -1f;
        }

        int count = 2 + rage;
        Vector2[] velocities = new Vector2[count];
        for (int i = 0; i < count; i++)
        {
            float speedX = direction * (3.2f + i * 1.6f);
            float speedY = 6.5f + Random.Range(-0.5f, 1.2f);
            velocities[i] = new Vector2(speedX, speedY);
        }

        return velocities;
    }

    void ShowLandingWarnings(Vector3 mouth, Vector2[] velocities)
    {
        foreach (Vector2 velocity in velocities)
        {
            Vector3 landingPoint;
            float landingSeconds;
            if (BossProjectile.TryPredictLanding(mouth, velocity, out landingPoint, out landingSeconds))
            {
                BossProjectile.ShowLandingMarker(landingPoint, SpitTelegraphSeconds + landingSeconds + 0.15f);
            }
        }
    }

    void SpitGlobs(Vector3 mouth, Vector2[] velocities)
    {
        RetroSfx.PlayShoot();
        foreach (Vector2 velocity in velocities)
        {
            BossProjectile.Spawn(mouth, velocity);
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
            KillPlayer(playerObject);
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
        bool feetNearTop = playerBounds.min.y >= bossBounds.max.y - 0.5f;
        bool above = playerBounds.center.y >= bossBounds.center.y + bossBounds.extents.y * 0.4f;
        bool falling = playerBody.linearVelocity.y <= 0.35f;
        return overlaps && feetNearTop && above && falling;
    }

    void TakeHit(GameObject playerObject)
    {
        // Always bounce the player off the head
        Rigidbody2D playerBody = playerObject.GetComponent<Rigidbody2D>();
        if (playerBody != null)
        {
            playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, 11f);
        }

        if (Time.time < invulnerableUntil)
        {
            return;
        }

        Health--;
        invulnerableUntil = Time.time + InvulnerableSeconds;
        RetroSfx.PlayStomp();
        JuiceManager.Shake(0.5f);
        JuiceManager.HitStop(0.1f);
        JuiceManager.Burst(transform.position + Vector3.up, new Color(1f, 0.5f, 0.95f), 18, 7f);

        if (Health <= 0)
        {
            Defeat();
        }
        else
        {
            JuiceManager.Popup(transform.position + Vector3.up * 2.4f, "あと " + Health + " かい!", Color.white, 1.3f);
            RetroSfx.PlayRoar();
        }
    }

    void KillPlayer(GameObject playerObject)
    {
        RespawnSystem.KillPlayer(playerObject, "ぶつかった!");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        // Bullets do not work on the boss - stomps only, like a proper boss.
        if (other.GetComponent<SlimeDamagingAttack>() == null)
        {
            return;
        }

        Destroy(other.gameObject);
        if (Time.time >= nextImmunePopupTime)
        {
            nextImmunePopupTime = Time.time + 1f;
            JuiceManager.Popup(transform.position + Vector3.up * 2.2f, "きかない!", new Color(0.8f, 0.8f, 0.9f), 1f);
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
        JuiceManager.Shake(0.7f);
        JuiceManager.HitStop(0.18f);
        JuiceManager.Confetti(transform.position + Vector3.up * 2f, 60);
        JuiceManager.Popup(transform.position + Vector3.up * 3f, "キングスライム げきは!!", new Color(1f, 0.9f, 0.3f), 1.7f);
        ScoreSystem.AddBonus(2000, transform.position + Vector3.up * 1.6f, "BOSS BONUS");

        if (barrier != null)
        {
            JuiceManager.Burst(barrier.transform.position, new Color(0.7f, 0.5f, 1f), 22, 7f);
            Destroy(barrier);
        }

        StartCoroutine(DeathAnimation());
    }

    IEnumerator DeathAnimation()
    {
        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        const float duration = 1.1f;

        while (elapsed < duration)
        {
            float progress = elapsed / duration;
            float wobble = 1f + Mathf.Sin(elapsed * 40f) * 0.12f * (1f - progress);
            transform.localScale = new Vector3(
                startScale.x * wobble * (1f - progress * 0.6f),
                startScale.y * (1f - progress) * wobble,
                startScale.z);

            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.Lerp(Color.white, new Color(1f, 1f, 1f, 0f), progress * progress);
            }

            if (Random.value < 0.2f)
            {
                JuiceManager.Burst(transform.position + (Vector3)(Random.insideUnitCircle * 1.5f),
                    Color.HSVToRGB(Random.value, 0.7f, 1f), 4, 4f);
            }

            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }
}
