using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class FunBootstrap : MonoBehaviour
{
    static FunBootstrap instance;

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Initialize()
    {
        if (instance != null)
        {
            return;
        }

        GameObject host = new GameObject("FunBootstrap");
        DontDestroyOnLoad(host);
        instance = host.AddComponent<FunBootstrap>();
        instance.SetUpScene();
    }

    void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        SetUpScene();
    }

    void SetUpScene()
    {
        LevelManager.DetectStageFromScene(SceneManager.GetActiveScene().name);
        ScoreSystem.Reset();

        if (FindAnyObjectByType<JuiceManager>() == null)
        {
            GameObject manager = new GameObject("FunManager");
            manager.AddComponent<JuiceManager>();
            manager.AddComponent<ScoreHud>();
            manager.AddComponent<RestartController>();
            manager.AddComponent<OpeningDirector>();
            manager.AddComponent<EndingDirector>();
            manager.AddComponent<MusicDirector>();
        }

        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null)
        {
            RespawnSystem.Reset(player.transform.position);

            if (player.GetComponent<PlayerBlinker>() == null)
            {
                player.AddComponent<PlayerBlinker>();
            }

            if (player.GetComponent<PlayerDash>() == null)
            {
                player.AddComponent<PlayerDash>();
            }

            if (player.GetComponent<SpecialAttack>() == null)
            {
                player.AddComponent<SpecialAttack>();
            }

            if (player.GetComponent<PlayerAttackShooter>() == null)
            {
                player.AddComponent<PlayerAttackShooter>();
            }
        }

        StartCoroutine(BuildLevelNextFrame());
    }

    IEnumerator BuildLevelNextFrame()
    {
        yield return null;
        LevelBuilder.Build(LevelManager.CurrentStage);
        // Final stage: no coins in the boss arena
        CoinSpawner.SpawnLevelCoins(LevelManager.CurrentStage >= LevelManager.MaxStage ? 36f : 50f);
    }
}
