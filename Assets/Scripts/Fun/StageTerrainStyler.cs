using UnityEngine;

public static class StageTerrainStyler
{
    struct PlatformPose
    {
        public readonly float x;
        public readonly float y;
        public readonly float scale;

        public PlatformPose(float x, float y, float scale)
        {
            this.x = x;
            this.y = y;
            this.scale = scale;
        }
    }

    static readonly PlatformPose[][] platformPosesByStage =
    {
        null,
        new[]
        {
            new PlatformPose(12f, -1.65f, 0.48f),
            new PlatformPose(27f, -0.82f, 0.5f),
            new PlatformPose(42f, -1.35f, 0.52f),
        },
        new[]
        {
            new PlatformPose(14f, -1.3f, 0.5f),
            new PlatformPose(38f, -0.1f, 0.54f),
            new PlatformPose(86f, -0.95f, 0.52f),
        },
        new[]
        {
            new PlatformPose(15f, -1.05f, 0.5f),
            new PlatformPose(37f, 0.25f, 0.54f),
            new PlatformPose(76f, -0.2f, 0.52f),
        },
        new[]
        {
            new PlatformPose(10f, -1.1f, 0.5f),
            new PlatformPose(35f, 0.2f, 0.52f),
            new PlatformPose(78f, 0.05f, 0.54f),
        },
        new[]
        {
            new PlatformPose(18f, -0.1f, 0.5f),
            new PlatformPose(46f, 0.38f, 0.52f),
            new PlatformPose(84f, -0.45f, 0.54f),
        },
    };

    static readonly float[][] decorXsByStage =
    {
        null,
        new[] { -3f, 6f, 17f, 32f, 48f },
        new[] { 3f, 18f, 43f, 63f, 91f },
        new[] { 2f, 22f, 42f, 68f, 91f },
        new[] { 5f, 16f, 36f, 61f, 88f },
        new[] { 2f, 29f, 52f, 76f, 93f },
    };

    public static void Apply(int stage)
    {
        stage = Mathf.Clamp(stage, 1, LevelManager.MaxStage);
        StyleGrounds(stage);
        StyleStaticPlatforms(stage);
        SpawnDecor(stage);
    }

    public static Sprite LoadPlatformSprite(int stage, bool lift)
    {
        stage = Mathf.Clamp(stage, 1, LevelManager.MaxStage);
        string suffix = lift ? "Lift" : "Platform";
        return Resources.Load<Sprite>("Terrain/Stage" + stage + suffix);
    }

    static void StyleGrounds(int stage)
    {
        Sprite groundSprite = Resources.Load<Sprite>("Terrain/Stage" + stage + "Ground");
        if (groundSprite == null)
        {
            return;
        }

        StyleTiledRenderer("Ground_0", groundSprite, 0);
        StyleTiledRenderer("Ground_Ext", groundSprite, 0);
    }

    static void StyleStaticPlatforms(int stage)
    {
        Sprite platformSprite = LoadPlatformSprite(stage, false);
        PlatformPose[] poses = platformPosesByStage[stage];

        for (int i = 0; i < 3; i++)
        {
            GameObject platform = GameObject.Find("Platform_" + i);
            if (platform == null)
            {
                continue;
            }

            PlatformPose pose = poses[i];
            platform.transform.position = new Vector3(pose.x, pose.y, platform.transform.position.z);
            platform.transform.localScale = new Vector3(pose.scale, pose.scale, 1f);

            SpriteRenderer renderer = platform.GetComponent<SpriteRenderer>();
            if (renderer != null && platformSprite != null)
            {
                renderer.sprite = platformSprite;
                renderer.drawMode = SpriteDrawMode.Sliced;
                renderer.size = new Vector2(12.54f, 1.49f);
                renderer.sortingOrder = 3;
            }
        }
    }

    static void StyleTiledRenderer(string objectName, Sprite sprite, int sortingOrder)
    {
        GameObject target = GameObject.Find(objectName);
        if (target == null)
        {
            return;
        }

        SpriteRenderer renderer = target.GetComponent<SpriteRenderer>();
        if (renderer == null)
        {
            return;
        }

        Vector2 previousSize = renderer.size;
        renderer.sprite = sprite;
        renderer.drawMode = SpriteDrawMode.Tiled;
        renderer.size = previousSize;
        renderer.sortingOrder = sortingOrder;
    }

    static void SpawnDecor(int stage)
    {
        if (GameObject.Find("__StageTerrainDecor") != null)
        {
            return;
        }

        GameObject root = new GameObject("__StageTerrainDecor");
        float[] xs = decorXsByStage[stage];

        for (int i = 0; i < xs.Length; i++)
        {
            Sprite prop = Resources.Load<Sprite>("StageDecor/Stage" + stage + "Prop" + ((i % 3) + 1));
            if (prop == null)
            {
                continue;
            }

            Vector2 groundPoint;
            if (!CoinSpawner.TryFindGround(xs[i], out groundPoint))
            {
                continue;
            }

            GameObject propObject = new GameObject("StageDecor_" + stage + "_" + i);
            propObject.transform.SetParent(root.transform);
            propObject.transform.position = new Vector3(groundPoint.x, groundPoint.y + 0.62f, 0f);
            float scale = 0.55f + (i % 3) * 0.12f;
            propObject.transform.localScale = new Vector3(scale, scale, 1f);

            SpriteRenderer renderer = propObject.AddComponent<SpriteRenderer>();
            renderer.sprite = prop;
            renderer.sortingOrder = 2;
            renderer.color = new Color(1f, 1f, 1f, 0.86f);
        }
    }
}
