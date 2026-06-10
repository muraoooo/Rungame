using System.Collections;
using UnityEngine;

public enum VariantEnemyKind
{
    Batkin,
    Togemaru,
    Pettan,
    Kabuton
}

[RequireComponent(typeof(Collider2D))]
public class VariantEnemy : MonoBehaviour
{
    public VariantEnemyKind kind;
    public float patrolDistance;
    public float patrolSpeed;
    public float stompBouncePower = 9f;
    public float bubbleIntervalSeconds = 2.2f;
    public float flightAmplitude = 0.55f;
    public float flightFrequency = 1.5f;

    Collider2D enemyCollider;
    Rigidbody2D body;
    SpriteRenderer spriteRenderer;
    Vector3 startPosition;
    int moveDirection = -1;
    float nextBubbleTime;
    bool defeated;

    public static VariantEnemy Spawn(VariantEnemyKind kind, Vector3 position, float patrolSpeed = 0f, float patrolDistance = 0f)
    {
        GameObject enemyObject = new GameObject(kind.ToString());
        enemyObject.transform.position = position;

        SpriteRenderer renderer = enemyObject.AddComponent<SpriteRenderer>();
        renderer.sprite = Resources.Load<Sprite>("Enemies/" + kind);
        renderer.sortingOrder = 9;

        BoxCollider2D collider = enemyObject.AddComponent<BoxCollider2D>();
        FitCollider(collider, renderer.sprite, kind);

        Rigidbody2D body = enemyObject.AddComponent<Rigidbody2D>();
        body.constraints = RigidbodyConstraints2D.FreezeRotation;
        body.interpolation = RigidbodyInterpolation2D.Interpolate;
        body.gravityScale = kind == VariantEnemyKind.Batkin ? 0f : 3.2f;
        body.bodyType = kind == VariantEnemyKind.Batkin ? RigidbodyType2D.Kinematic : RigidbodyType2D.Dynamic;

        VariantEnemy enemy = enemyObject.AddComponent<VariantEnemy>();
        enemy.kind = kind;
        enemy.patrolSpeed = patrolSpeed;
        enemy.patrolDistance = patrolDistance;
        return enemy;
    }

    void Start()
    {
        enemyCollider = GetComponent<Collider2D>();
        body = GetComponent<Rigidbody2D>();
        spriteRenderer = GetComponent<SpriteRenderer>();
        startPosition = transform.position;
        nextBubbleTime = Time.time + Random.Range(0.5f, 1.2f);
    }

    void FixedUpdate()
    {
        if (defeated || GameSession.HasEnded)
        {
            return;
        }

        if (kind == VariantEnemyKind.Batkin)
        {
            UpdateBatFlight();
            return;
        }

        if (kind == VariantEnemyKind.Pettan)
        {
            if (body != null)
            {
                body.linearVelocity = new Vector2(0f, body.linearVelocity.y);
            }
            return;
        }

        PatrolGround();
    }

    void Update()
    {
        if (defeated || GameSession.HasEnded || kind != VariantEnemyKind.Pettan)
        {
            return;
        }

        if (Time.time >= nextBubbleTime)
        {
            nextBubbleTime = Time.time + bubbleIntervalSeconds;
            Vector2 direction = FindPlayerDirection();
            PettanBubble.Spawn(transform.position + new Vector3(0.15f * direction.x, 0.35f, 0f), new Vector2(direction.x * 3.4f, 4.7f));
            RetroSfx.PlayShoot();
        }
    }

    void UpdateBatFlight()
    {
        float nextX = transform.position.x + moveDirection * patrolSpeed * Time.fixedDeltaTime;
        if (patrolDistance > 0f && Mathf.Abs(nextX - startPosition.x) > patrolDistance)
        {
            moveDirection *= -1;
            nextX = Mathf.Clamp(nextX, startPosition.x - patrolDistance, startPosition.x + patrolDistance);
        }

        float y = startPosition.y + Mathf.Sin(Time.time * flightFrequency) * flightAmplitude;
        body.MovePosition(new Vector2(nextX, y));
        FaceMoveDirection();
    }

    void PatrolGround()
    {
        if (body == null || patrolDistance <= 0f || patrolSpeed <= 0f)
        {
            return;
        }

        float nextX = body.position.x + moveDirection * patrolSpeed * Time.fixedDeltaTime;
        if (Mathf.Abs(nextX - startPosition.x) > patrolDistance)
        {
            moveDirection *= -1;
            nextX = Mathf.Clamp(nextX, startPosition.x - patrolDistance, startPosition.x + patrolDistance);
        }

        body.MovePosition(new Vector2(nextX, body.position.y));
        FaceMoveDirection();
    }

    void FaceMoveDirection()
    {
        if (spriteRenderer != null)
        {
            spriteRenderer.flipX = moveDirection < 0;
        }
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        HandleObject(collision.gameObject, collision);
    }

    void OnCollisionStay2D(Collision2D collision)
    {
        HandleObject(collision.gameObject, collision);
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        HandleObject(other.gameObject, null);
    }

    void HandleObject(GameObject other, Collision2D collision)
    {
        if (defeated || other == null)
        {
            return;
        }

        SlimeDamagingAttack attack = other.GetComponent<SlimeDamagingAttack>();
        if (attack == null)
        {
            attack = other.GetComponentInParent<SlimeDamagingAttack>();
        }

        if (attack != null)
        {
            HandleAttack(attack);
            return;
        }

        if (!other.CompareTag("Player"))
        {
            return;
        }

        if (ScoreSystem.IsFever)
        {
            Defeat(other.transform.position, false);
            return;
        }

        if (kind != VariantEnemyKind.Togemaru && IsPlayerStomping(other))
        {
            Defeat(other.transform.position, true);
            return;
        }

        RespawnSystem.KillPlayer(other);
    }

    void HandleAttack(SlimeDamagingAttack attack)
    {
        if (kind == VariantEnemyKind.Kabuton && HitsShield(attack))
        {
            Vector2 reflected = new Vector2(-attack.direction.x, attack.direction.y);
            attack.Fire(reflected, attack.speed);
            JuiceManager.Popup(transform.position + Vector3.up * 0.8f, "ガキン!", new Color(0.75f, 0.9f, 1f), 0.55f);
            RetroSfx.PlayShoot();
            return;
        }

        Defeat(attack.transform.position, false);
        Destroy(attack.gameObject);
    }

    public void DefeatBySpecial(Vector3 sourcePosition)
    {
        Defeat(sourcePosition, false);
    }

    bool HitsShield(SlimeDamagingAttack attack)
    {
        float attackDirection = Mathf.Sign(attack.direction.x);
        float frontDirection = moveDirection == 0 ? -1f : Mathf.Sign(moveDirection);
        return Mathf.Abs(attack.direction.x) > 0.01f && attackDirection == -frontDirection;
    }

    bool IsPlayerStomping(GameObject player)
    {
        Rigidbody2D playerBody = player.GetComponent<Rigidbody2D>();
        Collider2D playerCollider = player.GetComponent<Collider2D>();
        if (playerBody == null || playerCollider == null || enemyCollider == null)
        {
            return false;
        }

        Bounds playerBounds = playerCollider.bounds;
        Bounds enemyBounds = enemyCollider.bounds;
        bool overlaps = playerBounds.max.x > enemyBounds.min.x + 0.08f && playerBounds.min.x < enemyBounds.max.x - 0.08f;
        bool feetNearTop = playerBounds.min.y >= enemyBounds.max.y - 0.28f;
        bool falling = playerBody.linearVelocity.y <= 0.35f;
        return overlaps && feetNearTop && falling;
    }

    void Defeat(Vector3 sourcePosition, bool bouncePlayer)
    {
        if (defeated)
        {
            return;
        }

        defeated = true;
        ScoreSystem.RegisterStomp(transform.position);
        RetroSfx.PlayStomp();
        JuiceManager.Burst(transform.position, new Color(0.5f, 0.9f, 0.65f), 12, 4.8f);
        JuiceManager.Shake(0.2f);

        if (bouncePlayer)
        {
            GameObject player = GameObject.FindGameObjectWithTag("Player");
            Rigidbody2D playerBody = player != null ? player.GetComponent<Rigidbody2D>() : null;
            if (playerBody != null)
            {
                playerBody.linearVelocity = new Vector2(playerBody.linearVelocity.x, stompBouncePower);
            }
        }

        if (enemyCollider != null)
        {
            enemyCollider.enabled = false;
        }

        StartCoroutine(DefeatRoutine(sourcePosition));
    }

    IEnumerator DefeatRoutine(Vector3 sourcePosition)
    {
        float direction = transform.position.x >= sourcePosition.x ? 1f : -1f;
        if (body != null)
        {
            body.bodyType = RigidbodyType2D.Dynamic;
            body.gravityScale = 2.8f;
            body.linearVelocity = new Vector2(direction * 4.5f, 4.8f);
        }

        Vector3 startScale = transform.localScale;
        float elapsed = 0f;
        while (elapsed < 0.35f)
        {
            float wobble = Mathf.Sin(elapsed * 45f) * 0.08f;
            transform.localScale = new Vector3(startScale.x * (1f + wobble), startScale.y * (1f - Mathf.Abs(wobble)), startScale.z);
            elapsed += Time.deltaTime;
            yield return null;
        }

        Destroy(gameObject);
    }

    Vector2 FindPlayerDirection()
    {
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player == null)
        {
            return Vector2.left;
        }

        float direction = Mathf.Sign(player.transform.position.x - transform.position.x);
        return new Vector2(direction == 0f ? -1f : direction, 0f);
    }

    static void FitCollider(BoxCollider2D collider, Sprite sprite, VariantEnemyKind kind)
    {
        if (sprite == null)
        {
            collider.size = Vector2.one;
            return;
        }

        Bounds bounds = sprite.bounds;
        float width = kind == VariantEnemyKind.Batkin ? 0.78f : 0.72f;
        float height = kind == VariantEnemyKind.Togemaru ? 0.68f : 0.58f;
        collider.size = new Vector2(bounds.size.x * width, bounds.size.y * height);
        collider.offset = new Vector2(bounds.center.x, bounds.center.y - bounds.extents.y * 0.12f);
    }
}
