using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(Collider2D))]
public class Goal : MonoBehaviour
{
    public GameObject goalText;
    public bool snapToGroundOnStart = true;
    public float groundSnapDistance = 20f;

    bool isGoal;
    bool showedImageBanner;
    Collider2D goalCollider;

    void Start()
    {
        goalCollider = GetComponent<Collider2D>();

        if (snapToGroundOnStart)
        {
            SnapToGround();
        }

        if (goalText != null)
        {
            goalText.SetActive(false);
        }
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        ReachGoal(collision.gameObject);
    }

    void OnCollisionEnter2D(Collision2D collision)
    {
        ReachGoal(collision.gameObject);
    }

    void ReachGoal(GameObject player)
    {
        if (isGoal || !player.CompareTag("Player"))
        {
            return;
        }

        isGoal = true;

        if (goalText != null)
        {
            goalText.SetActive(true);

            Text text = goalText.GetComponent<Text>();
            if (text != null)
            {
                text.text = "ゴール";
            }
        }
        else
        {
            showedImageBanner = EndBannerUI.Show("UI/GoalBanner");
        }

        GameSession.EndGame(player, true);
        ScoreSystem.OnGoal(GameSession.ElapsedTime);
        RetroSfx.PlayFanfare();
        JuiceManager.Shake(0.35f);
        JuiceManager.Confetti(transform.position + Vector3.up * 2f, 70);
        JuiceManager.Confetti(player.transform.position + Vector3.up * 1.5f, 40);
    }

    void SnapToGround()
    {
        if (goalCollider == null)
        {
            return;
        }

        Bounds bounds = goalCollider.bounds;
        Vector2 rayStart = new Vector2(bounds.center.x, bounds.min.y - 0.02f);
        RaycastHit2D[] hits = Physics2D.RaycastAll(rayStart, Vector2.down, groundSnapDistance);

        foreach (RaycastHit2D hit in hits)
        {
            if (hit.collider == null || hit.collider == goalCollider || hit.collider.isTrigger || hit.normal.y < 0.5f)
            {
                continue;
            }

            float yOffset = hit.point.y - bounds.min.y;
            transform.position += new Vector3(0f, yOffset, 0f);
            return;
        }
    }

    void OnGUI()
    {
        if (!isGoal || goalText != null || showedImageBanner)
        {
            return;
        }

        GUIStyle style = new GUIStyle(GUI.skin.label);
        style.fontSize = 64;
        style.alignment = TextAnchor.MiddleCenter;
        style.normal.textColor = Color.yellow;

        GUI.Label(new Rect(0, 0, Screen.width, Screen.height), "ゴール", style);
    }
}
