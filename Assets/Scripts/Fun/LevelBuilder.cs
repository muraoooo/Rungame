using UnityEngine;

// Builds the selected stage at runtime on top of the base scene:
// spikes, springs, moving lifts, extra slimes (fast / hopping) and items.
public static class LevelBuilder
{
    public static void Build(int stage)
    {
        // Escape hatch for hand-edited scenes: put an empty GameObject named
        // "NoAutoBuild" in a stage scene and this auto-placement is skipped,
        // so everything can be placed by hand in the editor instead.
        if (GameObject.Find("NoAutoBuild") != null)
        {
            return;
        }

        switch (stage)
        {
            case 1:
                BuildStage1();
                break;
            case 2:
                BuildStage2();
                break;
            case 3:
                BuildStage3();
                break;
            case 4:
                BuildCaveStage();
                break;
            default:
                BuildBossStage();
                break;
        }
    }

    // 1-1: the original gentle layout, plus a spring to teach bouncing.
    static void BuildStage1()
    {
        // A tight coin row right at the start so the W special is charged
        // within the first seconds - the player tastes the big move early.
        for (int i = 0; i < 5; i++)
        {
            Vector2 starterGround;
            float x = -6.4f + i * 0.8f;
            if (CoinSpawner.TryFindGround(x, out starterGround))
            {
                Coin.Spawn(new Vector3(starterGround.x, starterGround.y + 0.95f, 0f));
            }
        }

        SpawnSpringOnGround(18f);
        SpawnCheckpointOnGround(24f);

        SpawnMedalOnGround(13f, 2.3f, 0);
        SpawnMedalOnGround(27f, 2.3f, 1);
        SpawnMedalOnGround(41f, 2.3f, 2);

        // First-timers get contextual coaching; veterans (anyone with a
        // recorded clear) never see these.
        if (IsFirstVisit(1))
        {
            SpawnHintOnGround(-5f, "→ キーで はしろう!");
            SpawnHintOnGround(-2f, "スライムは うえから ふもう!");
            SpawnHintOnGround(7f, "れんぞくで ふむと コンボ!");
            SpawnHintOnGround(14f, "SHIFT で ダッシュ!");
            SpawnHintOnGround(20f, "バネで おおジャンプ! くうちゅうで Q!");
            SpawnHintOnGround(44f, "コイン5まいで W ひっさつ!");
        }
    }

    // 1-2: spikes, faster enemies, a hopper, a lift and the coin magnet.
    static void BuildStage2()
    {
        TuneExistingSlimes(1.3f, 3.2f, 0.5f);

        SpawnSpikeOnGround(8f, 1.6f);
        SpawnSpikeOnGround(21f, 1.6f);
        SpawnSpikeOnGround(35f, 1.8f);

        SpawnVariantOnGround(VariantEnemyKind.Togemaru, 11.8f, 0f, 0f, 0.7f);
        // Second Togemaru moved off the spring at x38 - bouncing onto an
        // unstompable enemy was a trap. Now a static guard on open ground.
        SpawnVariantOnGround(VariantEnemyKind.Togemaru, 96f, 0f, 0f, 0.7f);
        SpawnExtraSlime(16f, 3.2f, 3f, false);
        SpawnExtraSlime(30f, 2f, 2.5f, false);
        SpawnExtraSlime(44f, 1.8f, 2f, true);

        if (IsFirstVisit(2))
        {
            SpawnHintOnGround(10.3f, "トゲボールは ふめない! Fで うとう!");
        }

        SpawnLiftOnGround(33f, 1.6f, 2.6f, 3.4f);
        SpawnSpringOnGround(38f);
        SpawnMagnetOnGround(25f, 1.3f);
        SpawnCheckpointOnGround(26.5f);

        SpawnMedalOnGround(8f, 2.3f, 0);
        SpawnMedalOnGround(21f, 1.5f, 1);
        SpawnMedalOnGround(33f, 4.4f, 2);
    }

    // 1-3: spike gauntlet, lots of fast and hopping slimes, two lifts.
    static void BuildStage3()
    {
        TuneExistingSlimes(1.6f, 4f, 0.25f);

        SpawnSpikeOnGround(6f, 2f);
        SpawnSpikeOnGround(14f, 2.2f);
        SpawnSpikeOnGround(20.4f, 2f);
        SpawnSpikeOnGround(42.8f, 2.4f);
        SpawnSpikeOnGround(47f, 2f);
        SpawnSpikeOnGround(52f, 2.2f);

        SpawnExtraSlime(23.4f, 3.6f, 3f, false);
        SpawnExtraSlime(26.8f, 2f, 2f, true);
        SpawnExtraSlime(30.2f, 3.6f, 3.5f, false);
        SpawnExtraSlime(33.6f, 2.2f, 2f, true);
        SpawnExtraSlime(37f, 2.8f, 2.5f, false);
        SpawnExtraSlime(50f, 2f, 1.5f, true);
        // Pettan nudged off the spike-22 landing zone; Kabuton moved out of
        // its old patrol (29.6-34.4) that walked through spike 30 and the
        // spring at 33 - now patrols clear ground past spike 38.
        SpawnVariantOnGround(VariantEnemyKind.Pettan, 18.8f, 0f, 0f, 0.65f);
        SpawnVariantOnGround(VariantEnemyKind.Kabuton, 96f, 1f, 1f, 0.72f);

        if (IsFirstVisit(3))
        {
            SpawnHintOnGround(96f, "カブトンは まえが かたい! うえから ふもう!");
        }

        SpawnLiftOnGround(20f, 1.6f, 3f, 3f);
        SpawnLiftOnGround(42f, 1.6f, 3.2f, 2.6f);
        SpawnSpringOnGround(12f);
        SpawnSpringOnGround(33f);
        SpawnMagnetOnGround(40f, 1.3f);
        SpawnCheckpointOnGround(16.5f);
        SpawnCheckpointOnGround(38.6f);

        SpawnMedalOnGround(12f, 4.6f, 0);
        SpawnMedalOnGround(20f, 4.4f, 1);
        SpawnMedalOnGround(45f, 1.5f, 2);
    }

    // 1-4: the cave. Deep darkness lit only by the player's lantern, torches
    // and the glow of magma pools. Teaches light-reading before the castle.
    static void BuildCaveStage()
    {
        CreateCaveDarkness();
        TuneExistingSlimes(1.45f, 3.6f, 0.4f);

        // The player carries a lantern through the dark
        GameObject player = GameObject.FindGameObjectWithTag("Player");
        if (player != null && player.GetComponent<PlayerLight>() == null)
        {
            player.AddComponent<PlayerLight>();
        }

        // Torches mark the safe path
        float[] torchPositions = { 0f, 8f, 16f, 24f, 32f, 40f, 46.5f };
        foreach (float x in torchPositions)
        {
            Vector2 torchGround;
            if (CoinSpawner.TryFindGround(x, out torchGround))
            {
                Torch.Spawn(torchGround);
            }
        }

        // Glowing magma pools - their light is both beauty and warning
        SpawnMagmaOnGround(11f, 2.4f);
        SpawnMagmaOnGround(20f, 3f);
        SpawnMagmaOnGround(29f, 2.4f);
        SpawnMagmaOnGround(36.5f, 3.6f);

        SpawnExtraSlime(14f, 2.6f, 2.2f, false);
        SpawnExtraSlime(26f, 2f, 1.8f, true);
        SpawnExtraSlime(43f, 3f, 2.5f, false);
        SpawnBatkin(24.8f, 1.6f, 0f, 0f);
        SpawnBatkin(96f, 1.8f, 1.4f, 2.8f);

        SpawnLiftOnGround(36.5f, 1.5f, 2.6f, 3f);
        SpawnSpringOnGround(31f);
        SpawnMagnetOnGround(24f, 1.3f);

        SpawnCheckpointOnGround(15.5f);
        SpawnCheckpointOnGround(31.8f);

        if (IsFirstVisit(4))
        {
            SpawnHintOnGround(8.5f, "マグマに さわると やけど!");
            SpawnHintOnGround(34f, "リフトで マグマを こえろ!");
        }

        // Medals glow over the magma - beauty guarding treasure
        SpawnMedalOnGround(20f, 1.8f, 0);
        SpawnMedalOnGround(29f, 1.8f, 1);
        SpawnMedalOnGround(36.5f, 4.6f, 2);
    }

    static void CreateCaveDarkness()
    {
        GameObject overlay = new GameObject("StageDarkOverlay");
        overlay.transform.position = new Vector3(22f, 2f, 0f);
        overlay.transform.localScale = new Vector3(95f, 45f, 1f);

        // The painted cave backdrop already carries its own darkness, so the
        // overlay only needs a light touch there - full strength would crush
        // the magma glow. Without art it does the darkening alone.
        bool hasStageArt = HasStageArt(4);

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite();
        renderer.color = new Color(0.02f, 0.03f, 0.1f, hasStageArt ? 0.38f : 0.68f);
        renderer.sortingOrder = 5;
    }

    static void SpawnMagmaOnGround(float x, float width)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            MagmaPool.Spawn(groundPoint, width);
        }
    }

    static void SpawnMedalOnGround(float x, float aboveGround, int index)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            StarMedal.Spawn(new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f), index);
        }
    }

    static void SpawnHintOnGround(float x, string text)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            TutorialHint.Spawn(new Vector3(groundPoint.x, groundPoint.y + 1f, 0f), text);
        }
    }

    static bool IsFirstVisit(int stage)
    {
        return PlayerPrefs.GetFloat("RungameBestTime_S" + stage, 0f) <= 0f;
    }

    // 1-5: the castle. Dark mood, precision spike rhythm, a lift crossing over
    // a long death zone, and the King Slime boss guarding the goal.
    static void BuildBossStage()
    {
        CreateDarkOverlay();
        TuneExistingSlimes(1.8f, 5f, 0.15f);

        // Rhythm section: evenly spaced spikes force precise jump timing
        SpawnSpikeOnGround(5f, 1.8f);
        SpawnSpikeOnGround(8.5f, 1.8f);
        SpawnSpikeOnGround(12f, 1.8f);
        SpawnExtraSlime(10f, 2f, 1.6f, true);

        SpawnExtraSlime(16f, 3.8f, 3f, false);
        SpawnExtraSlime(19f, 2.2f, 1.8f, true);

        // Death zone: a continuous spike field crossed by lifts
        // (or by spring + mid-air dash for the brave)
        SpawnSpringOnGround(21.5f);
        SpawnSpikeOnGround(24f, 2.4f);
        SpawnSpikeOnGround(26.4f, 2.4f);
        SpawnSpikeOnGround(28.8f, 2.4f);
        SpawnSpikeOnGround(31.2f, 2.4f);
        SpawnLiftOnGround(23f, 1.4f, 2.6f, 2.6f);
        SpawnLiftOnGround(28f, 1.4f, 2.8f, 2.4f);

        // Last breath before the arena
        SpawnSpikeOnGround(33.6f, 1.8f);
        SpawnSpikeOnGround(37f, 1.8f);
        // Old Togemaru at 35.7 sat in the 0.7-unit gap between the two
        // spikes - physically impassable on the ground. Now a static gate
        // guard before the arena. Kabuton is static so it no longer patrols
        // over the checkpoint (38.5) and the spring (41). Batkin removed:
        // boss globs + flying enemy + spring arcs was overload.
        SpawnVariantOnGround(VariantEnemyKind.Togemaru, 44.3f, 0f, 0f, 0.7f);
        SpawnVariantOnGround(VariantEnemyKind.Kabuton, 42.4f, 0f, 0f, 0.72f);

        SpawnCheckpointOnGround(17f);
        SpawnCheckpointOnGround(38.5f);

        SpawnMedalOnGround(8.5f, 2.3f, 0);
        SpawnMedalOnGround(28f, 4.8f, 1);
        SpawnMedalOnGround(47f, 3.6f, 2);

        // Boss arena: barrier seals the goal until the King falls.
        // A spring lets the player bounce up for readable head stomps.
        SpawnSpringOnGround(41f);
        GameObject barrier = CreateBossBarrier(51f);
        Vector2 arenaGround;
        if (CoinSpawner.TryFindGround(47f, out arenaGround))
        {
            BossSlime.Spawn(new Vector3(arenaGround.x, arenaGround.y + 2.5f, 0f), barrier);
        }
    }

    static void CreateDarkOverlay()
    {
        GameObject overlay = new GameObject("StageDarkOverlay");
        overlay.transform.position = new Vector3(22f, 2f, 0f);
        overlay.transform.localScale = new Vector3(95f, 45f, 1f);

        bool hasStageArt = HasStageArt(5);

        SpriteRenderer renderer = overlay.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite();
        renderer.color = new Color(0.12f, 0.04f, 0.22f, hasStageArt ? 0.28f : 0.48f);
        renderer.sortingOrder = 5;
    }

    static bool HasStageArt(int stage)
    {
        string prefix = "StageArt/Stage" + stage + "/";
        return Resources.Load<Sprite>(prefix + "sky") != null
            || Resources.Load<Sprite>(prefix + "far") != null
            || Resources.Load<Sprite>(prefix + "mid") != null
            || Resources.Load<Sprite>(prefix + "near") != null;
    }

    static GameObject CreateBossBarrier(float x)
    {
        Vector2 groundPoint;
        if (!CoinSpawner.TryFindGround(x, out groundPoint))
        {
            groundPoint = new Vector2(x, -2.8f);
        }

        GameObject barrier = new GameObject("BossBarrier");
        barrier.transform.position = new Vector3(groundPoint.x, groundPoint.y + 4f, 0f);
        barrier.transform.localScale = new Vector3(0.7f, 9f, 1f);

        SpriteRenderer renderer = barrier.AddComponent<SpriteRenderer>();
        renderer.sprite = CreateSolidSprite();
        renderer.color = new Color(0.6f, 0.3f, 1f, 0.55f);
        renderer.sortingOrder = 11;

        barrier.AddComponent<BoxCollider2D>();
        barrier.AddComponent<BarrierPulse>();
        return barrier;
    }

    static Sprite solidSprite;

    static Sprite CreateSolidSprite()
    {
        if (solidSprite != null)
        {
            return solidSprite;
        }

        Texture2D texture = new Texture2D(4, 4, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[16];
        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = Color.white;
        }
        texture.SetPixels(pixels);
        texture.Apply();
        solidSprite = Sprite.Create(texture, new Rect(0f, 0f, 4f, 4f), new Vector2(0.5f, 0.5f), 4f);
        return solidSprite;
    }

    static void TuneExistingSlimes(float speedMultiplier, float moveSeconds, float pauseSeconds)
    {
        GameOver[] slimes = Object.FindObjectsByType<GameOver>(FindObjectsSortMode.None);
        foreach (GameOver slime in slimes)
        {
            slime.patrolSpeed *= speedMultiplier;
            slime.moveSecondsBeforeIdle = moveSeconds;
            slime.idlePauseSeconds = pauseSeconds;
        }
    }

    static void SpawnExtraSlime(float x, float patrolSpeed, float patrolDistance, bool hopper)
    {
        Vector2 groundPoint;
        if (!CoinSpawner.TryFindGround(x, out groundPoint))
        {
            return;
        }

        GameObject slimeObject = new GameObject("SlimeEnemy_Extra_" + x);
        slimeObject.transform.position = new Vector3(groundPoint.x, groundPoint.y + 0.6f, 0f);

        SpriteRenderer renderer = slimeObject.AddComponent<SpriteRenderer>();
        Sprite[] idleSprites = Resources.LoadAll<Sprite>("Slime/Idle");
        if (idleSprites != null && idleSprites.Length > 0)
        {
            renderer.sprite = idleSprites[0];
        }
        renderer.sortingOrder = 8;

        slimeObject.AddComponent<BoxCollider2D>();

        GameOver enemy = slimeObject.AddComponent<GameOver>();
        enemy.patrolSpeed = patrolSpeed;
        enemy.patrolDistance = patrolDistance;
        enemy.moveSecondsBeforeIdle = hopper ? 0f : 3f;
        enemy.idlePauseSeconds = 0.3f;

        if (hopper)
        {
            SlimeHopper hop = slimeObject.AddComponent<SlimeHopper>();
            hop.hopPower = 6.5f;
            hop.hopIntervalSeconds = 1.5f;
        }
    }

    static void SpawnVariantOnGround(VariantEnemyKind kind, float x, float patrolSpeed, float patrolDistance, float aboveGround)
    {
        Vector2 groundPoint;
        if (!CoinSpawner.TryFindGround(x, out groundPoint))
        {
            return;
        }

        VariantEnemy.Spawn(kind, new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f), patrolSpeed, patrolDistance);
    }

    static void SpawnBatkin(float x, float aboveGround, float patrolSpeed, float patrolDistance)
    {
        Vector2 groundPoint;
        if (!CoinSpawner.TryFindGround(x, out groundPoint))
        {
            return;
        }

        VariantEnemy.Spawn(VariantEnemyKind.Batkin, new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f), patrolSpeed, patrolDistance);
    }

    static void SpawnSpikeOnGround(float x, float width)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            Spike.Spawn(groundPoint, width);
        }
    }

    static void SpawnSpringOnGround(float x)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            Spring.Spawn(groundPoint);
        }
    }

    static void SpawnLiftOnGround(float x, float aboveGround, float amplitude, float period)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            MovingPlatform.Spawn(new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f), amplitude, period);
        }
    }

    static void SpawnCheckpointOnGround(float x)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            CheckpointFlag.Spawn(groundPoint);
        }
    }

    static void SpawnMagnetOnGround(float x, float aboveGround)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            MagnetItem.Spawn(new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f));
        }
    }
}

// Slow magical pulse for the boss barrier.
public class BarrierPulse : MonoBehaviour
{
    SpriteRenderer spriteRenderer;
    Color baseColor;

    void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
        if (spriteRenderer != null)
        {
            baseColor = spriteRenderer.color;
        }
    }

    void Update()
    {
        if (spriteRenderer == null)
        {
            return;
        }

        float pulse = 0.4f + Mathf.PingPong(Time.unscaledTime * 0.5f, 0.3f);
        spriteRenderer.color = new Color(baseColor.r, baseColor.g, baseColor.b, pulse);
    }
}
