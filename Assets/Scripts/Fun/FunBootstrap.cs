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

        TuneCamera();

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

    // Wider view + forward look-ahead. The scene ships with orthographic
    // size 5 and a centered camera - too tight to react at run speed, and
    // a classic action-game NG (you cannot see what you are running into).
    void TuneCamera()
    {
        Camera camera = Camera.main;
        if (camera == null || !camera.orthographic)
        {
            return;
        }

        camera.orthographicSize = 6.1f;

        CameraFollowClamp2 follow = camera.GetComponent<CameraFollowClamp2>();
        if (follow != null)
        {
            follow.offset = new Vector3(2.2f, follow.offset.y, follow.offset.z);
        }
    }

    IEnumerator BuildLevelNextFrame()
    {
        yield return null;
        StageArtBootstrap.Build(LevelManager.CurrentStage);
        LevelStretcher.Apply(LevelManager.CurrentStage);
        LevelBuilder.Build(LevelManager.CurrentStage);
        CoinSpawner.SpawnLevelCoins(LevelStretcher.CoinEndXForStage(LevelManager.CurrentStage));
    }
}
