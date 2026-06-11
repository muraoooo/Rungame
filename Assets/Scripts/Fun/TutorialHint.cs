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
        hint.text = NormalizeText(text);
        return hint;
    }

    static string NormalizeText(string source)
    {
        switch (source)
        {
            case "→ キーで はしろう!":
                return "→で はしる!";
            case "スライムは うえから ふもう!":
                return "うえから ふむ!";
            case "れんぞくで ふむと コンボ!":
                return "れんぞくで ふむ!";
            case "SHIFT で ダッシュ!":
                return "SHIFTで ダッシュ!";
            case "バネで おおジャンプ! くうちゅうで Q!":
                return "Qで くうちゅうアクション!";
            case "コイン5まいで W ひっさつ!":
            case "コイン10まいで W ひっさつ!":
                return "Wで ひっさつ!";
            case "トゲボールは ふめない! Fで うとう!":
                return "Fで トゲを うつ!";
            case "カブトンは まえが かたい! うえから ふもう!":
                return "うえから カブトンを ふむ!";
            case "マグマに さわると やけど!":
                return "マグマを とびこえる!";
            case "リフトで マグマを こえろ!":
                return "リフトで こえる!";
            default:
                return source;
        }
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
