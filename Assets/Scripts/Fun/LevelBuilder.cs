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
        SpawnSpringOnGround(18f);
        SpawnCheckpointOnGround(24f);

        SpawnMedalOnGround(13f, 2.3f, 0);
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
            SpawnHintOnGround(8f, "コイン5まいで W ひっさつ!");
        }
    }

    // 1-2: spikes, faster enemies, a hopper, a lift and the coin magnet.
    static void BuildStage2()
    {
        PlaceGoalOnGround(96f);
        TuneExistingSlimes(1.3f, 3.2f, 0.5f);

        // Act 1 (0-30): keep the learned opening mostly intact.
        SpawnSpikeOnGround(8f, 1.6f);
        SpawnSpikeOnGround(21f, 1.6f);

        SpawnVariantOnGround(VariantEnemyKind.Togemaru, 11.8f, 0f, 0f, 0.7f);
        SpawnExtraSlime(16f, 3.2f, 3f, false);
        SpawnExtraSlime(30f, 2f, 2.5f, false);

        if (IsFirstVisit(2))
        {
            SpawnHintOnGround(10.3f, "トゲボールは ふめない! Fで うとう!");
        }

        SpawnMagnetOnGround(25f, 1.3f);
        SpawnCheckpointOnGround(25f);

        SpawnMedalOnGround(8f, 2.3f, 0);

        // Act 2 (30-65): formations.
        SpawnShieldAndTurret(34f);
        SpawnPincerHoppers(54f, 61f);
        SpawnCheckpointOnGround(50f);

        // Act 3 (65-85): thorn wave, crossed from above by two lifts.
        SpawnSpikeWave(66f, 5, 1.2f, 2f);
        SpawnLiftOnGround(66.6f, 2.1f, 2.2f, 2.8f);
        SpawnLiftOnGround(73.4f, 2.3f, 2.4f, 2.6f);
        SpawnCheckpointOnGround(75f);

        SpawnMedalOnGround(91f, 2.6f, 2);
    }

    // 1-3: spike gauntlet, lots of fast and hopping slimes, two lifts.
    static void BuildStage3()
    {
        PlaceGoalOnGround(96f);
        TuneExistingSlimes(1.6f, 4f, 0.25f);

        // Act 1 (0-30): familiar spike/slime grammar.
        SpawnSpikeOnGround(6f, 2f);
        SpawnSpikeOnGround(14f, 2.2f);
        SpawnSpikeOnGround(20.4f, 2f);
        SpawnExtraSlime(23.4f, 3.6f, 3f, false);
        SpawnExtraSlime(26.8f, 2f, 2f, true);
        // Pettan guards the approach to the combo row from a safe distance.
        // Kabuton makes its proper debut in stage 5 instead - the combo row
        // already fills this stage's complexity budget.
        SpawnVariantOnGround(VariantEnemyKind.Pettan, 18.8f, 0f, 0f, 0.65f);

        SpawnLiftOnGround(20f, 1.6f, 3f, 3f);
        SpawnSpringOnGround(12f);
        SpawnMagnetOnGround(40f, 1.3f);
        SpawnCheckpointOnGround(25f);

        SpawnMedalOnGround(20f, 4.4f, 1);

        // Act 2 (30-65): formations.
        SpawnShieldAndTurret(34f);
        SpawnPincerHoppers(49f, 56f);
        SpawnCheckpointOnGround(50f);

        // Act 3 (65-85): 8-stomp combo festival. Starts slightly before 65
        // so the final enemy stays before the enemy-free reward lane.
        for (int i = 0; i < 4; i++)
        {
            SpawnExtraSlime(60f + i * 5.2f, 2.2f, 1.1f, false);
        }
        SpawnCheckpointOnGround(84f);

        SpawnMedalOnGround(91f, 2.7f, 2);
    }

    // 1-4: the cave. Deep darkness lit only by the player's lantern, torches
    // and the glow of magma pools. Teaches light-reading before the castle.
    static void BuildCaveStage()
    {
        PlaceGoalOnGround(96f);
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

        SpawnExtraSlime(14f, 2.6f, 2.2f, false);
        SpawnExtraSlime(26f, 2f, 1.8f, true);
        SpawnBatkin(24.8f, 1.6f, 0f, 0f);

        SpawnMagnetOnGround(24f, 1.3f);

        SpawnCheckpointOnGround(25f);

        if (IsFirstVisit(4))
        {
            SpawnHintOnGround(8.5f, "マグマに さわると やけど!");
            SpawnHintOnGround(34f, "盾のうしろに 砲台! まず観察!");
        }

        // Medals glow over the magma - beauty guarding treasure.
        SpawnMedalOnGround(29f, 1.8f, 1);

        // Act 2 (30-65): formations.
        SpawnShieldAndTurret(34f);
        SpawnBatkinWave(44f);
        SpawnPincerHoppers(57f, 63f);
        SpawnCheckpointOnGround(50f);

        // Act 3 (65-85): magma waterfall, lift -> spring -> lift.
        SpawnMagmaOnGround(66f, 6f);
        SpawnMagmaOnGround(75f, 6f);
        SpawnLiftOnGround(63.5f, 1.7f, 2.5f, 2.8f);
        SpawnSpringOnGround(70.5f);
        SpawnLiftOnGround(79f, 1.8f, 2.8f, 2.8f);
        SpawnCheckpointOnGround(84f);

        SpawnMedalOnGround(91f, 3.1f, 2);
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
        float goalX = LevelStretcher.TargetGoalX(LevelManager.CurrentStage);
        PlaceGoalOnGround(goalX);
        CreateDarkOverlay();
        TuneExistingSlimes(1.8f, 5f, 0.15f);

        // Act 1 (0-30): keep the learned castle rhythm.
        SpawnSpikeOnGround(5f, 1.8f);
        SpawnSpikeOnGround(8.5f, 1.8f);
        SpawnSpikeOnGround(12f, 1.8f);
        SpawnExtraSlime(10f, 2f, 1.6f, true);

        SpawnExtraSlime(16f, 3.8f, 3f, false);

        // Death zone: a continuous spike field crossed by lifts
        // (or by spring + mid-air dash for the brave)
        SpawnSpringOnGround(21.5f);
        SpawnSpikeOnGround(24f, 2.4f);
        SpawnSpikeOnGround(26.4f, 2.4f);
        SpawnSpikeOnGround(28.8f, 2.4f);
        SpawnSpikeOnGround(31.2f, 2.4f);
        SpawnLiftOnGround(23f, 1.4f, 2.6f, 2.6f);
        SpawnLiftOnGround(28f, 1.4f, 2.8f, 2.4f);

        SpawnCheckpointOnGround(25f);

        // Act 2 (30-65): formations.
        SpawnShieldAndTurret(34f);
        SpawnBatkin(45f, 1.6f, 1.1f, 1.6f);
        SpawnPincerHoppers(57f, 63f);
        SpawnCheckpointOnGround(50f);

        if (IsFirstVisit(5))
        {
            SpawnHintOnGround(33f, "カブトンは まえが かたい! うえから ふもう!");
        }

        // Act 3 (65-85): two castle gates, then a final barrier trial.
        SpawnVariantOnGround(VariantEnemyKind.Togemaru, 66f, 0f, 0f, 0.7f);
        SpawnSpikeOnGround(68f, 1.8f);
        SpawnVariantOnGround(VariantEnemyKind.Kabuton, 80f, 0f, 0f, 0.72f);
        SpawnLiftOnGround(82f, 1.7f, 2.4f, 2.6f);
        SpawnCheckpointOnGround(70f);
        SpawnCheckpointOnGround(84f);

        // Reward lane before the boss arena: no normal enemies here.
        SpawnMedalOnGround(goalX - 16f, 3.2f, 2);

        // Boss arena: barrier seals the goal until the King falls.
        // A spring lets the player bounce up for readable head stomps.
        SpawnSpringOnGround(goalX - 9.5f);
        GameObject barrier = CreateBossBarrier(goalX - 2.2f);
        Vector2 arenaGround;
        if (CoinSpawner.TryFindGround(goalX - 6f, out arenaGround))
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

    static void PlaceGoalOnGround(float x)
    {
        Goal goal = Object.FindAnyObjectByType<Goal>();
        if (goal == null)
        {
            return;
        }

        Vector2 newGroundPoint;
        if (!CoinSpawner.TryFindGround(x, out newGroundPoint))
        {
            return;
        }

        Vector2 currentGroundPoint;
        float yOffset = 1.45f;
        if (CoinSpawner.TryFindGround(goal.transform.position.x, out currentGroundPoint))
        {
            yOffset = goal.transform.position.y - currentGroundPoint.y;
        }

        goal.transform.position = new Vector3(newGroundPoint.x, newGroundPoint.y + yOffset, goal.transform.position.z);
    }

    static void SpawnShieldAndTurret(float frontX)
    {
        SpawnVariantOnGround(VariantEnemyKind.Kabuton, frontX, 0f, 0f, 0.72f);
        SpawnVariantOnGround(VariantEnemyKind.Pettan, frontX + 2.5f, 0f, 0f, 0.65f);
    }

    static void SpawnBatkinWave(float startX)
    {
        SpawnBatkin(startX, 1.35f, 1.1f, 1.6f);
        SpawnBatkin(startX + 6f, 1.65f, 1.2f, 1.8f);
    }

    static void SpawnPincerHoppers(float leftX, float rightX)
    {
        SpawnExtraSlime((leftX + rightX) * 0.5f, 2.5f, 1.8f, true);
    }

    static void SpawnSpikeWave(float startX, int count, float spacing, float width)
    {
        for (int i = 0; i < count; i++)
        {
            SpawnSpikeOnGround(startX + i * spacing, width);
        }
    }

    static void SpawnRewardCoinArc(float startX, float endX, int count, float height)
    {
        if (count <= 1)
        {
            return;
        }

        for (int i = 0; i < count; i++)
        {
            float t = i / (float)(count - 1);
            float x = Mathf.Lerp(startX, endX, t);
            Vector2 groundPoint;
            if (!CoinSpawner.TryFindGround(x, out groundPoint))
            {
                continue;
            }

            float arc = Mathf.Sin(t * Mathf.PI) * height;
            Coin.Spawn(new Vector3(groundPoint.x, groundPoint.y + 1.25f + arc, 0f));
        }
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
