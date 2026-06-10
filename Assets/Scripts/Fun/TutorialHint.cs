using UnityEngine;

// One-shot teaching popup that fires when the player walks into its zone.
// Spawned only for players who have not cleared the stage yet, so veterans
// never see them.
public class TutorialHint : MonoBehaviour
{
    string text;
    bool shown;

    public static TutorialHint Spawn(Vector3 position, string text)
    {
        GameObject hintObject = new GameObject("TutorialHint");
        hintObject.transform.position = position;

        CircleCollider2D collider = hintObject.AddComponent<CircleCollider2D>();
        collider.isTrigger = true;
        collider.radius = 1.6f;

        TutorialHint hint = hintObject.AddComponent<TutorialHint>();
        hint.text = text;
        return hint;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (shown || !other.CompareTag("Player"))
        {
            return;
        }

        shown = true;
        JuiceManager.Popup(transform.position + Vector3.up * 2.4f, text, new Color(0.75f, 0.95f, 1f), 1.15f);
        Destroy(gameObject, 0.5f);
    }
}
