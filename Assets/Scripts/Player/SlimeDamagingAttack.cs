using UnityEngine;

[RequireComponent(typeof(Collider2D))]
public class SlimeDamagingAttack : MonoBehaviour
{
    public float speed = 9f;
    public float lifeSeconds = 1.4f;
    public Vector2 direction = Vector2.right;

    Rigidbody2D rb2d;

    void Awake()
    {
        rb2d = GetComponent<Rigidbody2D>();

        if (rb2d == null)
        {
            rb2d = gameObject.AddComponent<Rigidbody2D>();
        }

        rb2d.gravityScale = 0f;
        rb2d.collisionDetectionMode = CollisionDetectionMode2D.Continuous;
        rb2d.constraints = RigidbodyConstraints2D.FreezeRotation;

        Collider2D attackCollider = GetComponent<Collider2D>();
        attackCollider.isTrigger = true;
    }

    void OnEnable()
    {
        ApplyVelocity();
    }

    void Update()
    {
        lifeSeconds -= Time.deltaTime;

        if (lifeSeconds <= 0f)
        {
            Destroy(gameObject);
        }
    }

    public void Fire(Vector2 fireDirection, float fireSpeed)
    {
        direction = fireDirection.sqrMagnitude > 0.01f ? fireDirection.normalized : Vector2.right;
        speed = fireSpeed;
        ApplyVelocity();
    }

    void ApplyVelocity()
    {
        if (rb2d != null)
        {
            rb2d.linearVelocity = direction.normalized * speed;
        }
    }
}
