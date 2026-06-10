using UnityEngine;

// Makes a slime hop periodically. Works alongside the GameOver patrol,
// which only overrides the X position and leaves vertical velocity intact.
[RequireComponent(typeof(Rigidbody2D))]
public class SlimeHopper : MonoBehaviour
{
    public float hopPower = 6.5f;
    public float hopIntervalSeconds = 1.5f;

    Rigidbody2D body;
    float nextHopTime;

    void Start()
    {
        body = GetComponent<Rigidbody2D>();
        nextHopTime = Time.time + hopIntervalSeconds * (0.5f + Random.value);
    }

    void FixedUpdate()
    {
        if (Time.time < nextHopTime)
        {
            return;
        }

        // Only hop when roughly on the ground.
        if (Mathf.Abs(body.linearVelocity.y) > 0.25f)
        {
            return;
        }

        body.linearVelocity = new Vector2(body.linearVelocity.x, hopPower);
        nextHopTime = Time.time + hopIntervalSeconds;
    }
}
