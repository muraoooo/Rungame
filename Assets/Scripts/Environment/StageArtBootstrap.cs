using UnityEngine;

public static class StageArtBootstrap
{
    const string RootName = "__GeneratedStageArt";

    public static void Build(int stage)
    {
        GameObject existing = GameObject.Find(RootName);
        if (existing != null)
        {
            Object.Destroy(existing);
        }

        GameObject root = new GameObject(RootName);
        root.hideFlags = HideFlags.DontSave;

        // A stage art layer replaces the legacy grassland layers, which would
        // otherwise draw in front of it (Background sits at order -10, above
        // the sky/far/mid layers at -40/-35/-30). Some stages ship only one
        // full-bleed layer, so check every supported layer name.
        if (HasStageArt(stage))
        {
            DisableLegacyBackdrop();
        }

        BuildBackdropLayer(root.transform, stage, "sky", -40, 0.05f, 1f, 0f, 1.65f, 1f);
        BuildBackdropLayer(root.transform, stage, "far", -35, -0.08f, 0.92f, 0.05f, 1.48f, 1f);
        BuildBackdropLayer(root.transform, stage, "mid", -30, -0.5f, 0.86f, 0.16f, 1.4f, 1f);
        BuildBackdropLayer(root.transform, stage, "near", -18, -1.7f, 1f, 0.62f, 1.32f, 1f);

        BuildPropLayer(root.transform, stage, "Far", -32, 8, 1.2f, 1.1f, 0.07f, 101);
        BuildPropLayer(root.transform, stage, "Mid", -24, 10, -0.7f, 1f, 0.18f, 202);
        BuildPropLayer(root.transform, stage, "Near", -16, 7, -2.1f, 1.15f, 0.75f, 303);
        BuildPropLayer(root.transform, stage, "Near", 18, 5, -3.05f, 1.85f, 1.05f, 404);
    }

    static void DisableLegacyBackdrop()
    {
        string[] legacyNames = { "Background", "ForestParallax", "FarCloudParallax", "ForegroundTreeParallax" };
        foreach (string legacyName in legacyNames)
        {
            GameObject legacy = GameObject.Find(legacyName);
            if (legacy != null)
            {
                legacy.SetActive(false);
            }
        }
    }

    static bool HasStageArt(int stage)
    {
        string prefix = "StageArt/Stage" + stage + "/";
        string[] layerNames = { "sky", "far", "mid", "near" };
        foreach (string layerName in layerNames)
        {
            if (Resources.Load<Sprite>(prefix + layerName) != null)
            {
                return true;
            }
        }

        return false;
    }

    static void BuildBackdropLayer(Transform root, int stage, string name, int sortingOrder, float y, float scale, float parallaxStrength, float heightScale, float verticalParallaxStrength)
    {
        Sprite sprite = Resources.Load<Sprite>("StageArt/Stage" + stage + "/" + name);
        if (sprite == null)
        {
            return;
        }

        GameObject layer = new GameObject("StageArt_" + name);
        layer.transform.SetParent(root);

        StageArtTiledLayer tiled = layer.AddComponent<StageArtTiledLayer>();
        tiled.sprite = sprite;
        tiled.sortingOrder = sortingOrder;
        tiled.baseY = y;
        tiled.scale = scale;
        tiled.parallaxStrength = parallaxStrength;
        tiled.heightScale = heightScale;
        tiled.verticalParallaxStrength = verticalParallaxStrength;
    }

    static void BuildPropLayer(Transform root, int stage, string folder, int sortingOrder, int elementCount, float baseY, float scale, float parallaxStrength, int seed)
    {
        string resourceFolder = "StageArt/Stage" + stage + "/Props/" + folder;
        Sprite[] sprites = Resources.LoadAll<Sprite>(resourceFolder);
        if (sprites == null || sprites.Length == 0)
        {
            return;
        }

        GameObject layer = new GameObject("StageArt_Props_" + folder);
        layer.transform.SetParent(root);

        EnvironmentParallaxLoop parallax = layer.AddComponent<EnvironmentParallaxLoop>();
        parallax.sprites = sprites;
        parallax.sortingOrder = sortingOrder;
        parallax.elementCount = elementCount;
        parallax.baseY = baseY;
        parallax.yVariation = folder == "Far" ? 0.55f : 0.35f;
        parallax.minSpacing = folder == "Near" ? 3.2f : 5.2f;
        parallax.maxSpacing = folder == "Near" ? 6.8f : 9f;
        parallax.scaleRange = new Vector2(scale * 0.82f, scale * 1.18f);
        parallax.parallaxStrength = parallaxStrength;
        parallax.idleDriftSpeed = 0f;
        parallax.randomSeed = seed + stage;
        parallax.enabled = false;
        parallax.enabled = true;
    }
}
