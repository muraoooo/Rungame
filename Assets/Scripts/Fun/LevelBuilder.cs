using UnityEngine;

// Builds the selected stage at runtime on top of the base scene:
// spikes, springs, moving lifts, extra slimes (fast / hopping) and items.
public static class LevelBuilder
{
    public static void Build(int stage)
    {
        switch (stage)
        {
            case 1:
                BuildStage1();
                break;
            case 2:
                BuildStage2();
                break;
            default:
                BuildStage3();
                break;
        }
    }

    // 1-1: the original gentle layout, plus a spring to teach bouncing.
    static void BuildStage1()
    {
        SpawnSpringOnGround(18f);
    }

    // 1-2: spikes, faster enemies, a hopper, a lift and the coin magnet.
    static void BuildStage2()
    {
        TuneExistingSlimes(1.3f, 3.2f, 0.5f);

        SpawnSpikeOnGround(8f, 1.6f);
        SpawnSpikeOnGround(21f, 1.6f);
        SpawnSpikeOnGround(35f, 1.8f);

        SpawnExtraSlime(16f, 3.2f, 3f, false);
        SpawnExtraSlime(30f, 2f, 2.5f, false);
        SpawnExtraSlime(44f, 1.8f, 2f, true);

        SpawnLiftOnGround(33f, 1.6f, 2.6f, 3.4f);
        SpawnSpringOnGround(38f);
        SpawnMagnetOnGround(25f, 1.3f);
    }

    // 1-3: spike gauntlet, lots of fast and hopping slimes, two lifts.
    static void BuildStage3()
    {
        TuneExistingSlimes(1.6f, 4f, 0.25f);

        SpawnSpikeOnGround(6f, 2f);
        SpawnSpikeOnGround(14f, 2.2f);
        SpawnSpikeOnGround(22f, 2f);
        SpawnSpikeOnGround(30f, 2.4f);
        SpawnSpikeOnGround(38f, 2f);
        SpawnSpikeOnGround(45f, 2.2f);

        SpawnExtraSlime(10f, 3.6f, 3f, false);
        SpawnExtraSlime(18f, 2f, 2f, true);
        SpawnExtraSlime(28f, 3.6f, 3.5f, false);
        SpawnExtraSlime(36f, 2.2f, 2f, true);
        SpawnExtraSlime(47f, 2.8f, 2.5f, false);
        SpawnExtraSlime(50f, 2f, 1.5f, true);

        SpawnLiftOnGround(20f, 1.6f, 3f, 3f);
        SpawnLiftOnGround(42f, 1.6f, 3.2f, 2.6f);
        SpawnSpringOnGround(12f);
        SpawnSpringOnGround(33f);
        SpawnMagnetOnGround(40f, 1.3f);
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

    static void SpawnMagnetOnGround(float x, float aboveGround)
    {
        Vector2 groundPoint;
        if (CoinSpawner.TryFindGround(x, out groundPoint))
        {
            MagnetItem.Spawn(new Vector3(groundPoint.x, groundPoint.y + aboveGround, 0f));
        }
    }
}
