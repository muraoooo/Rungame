using System.Collections.Generic;
using UnityEngine;

public class CloudParallaxLoop : MonoBehaviour
{
    [Header("追いかける対象")]
    public Transform player;
    public Camera targetCamera;

    [Header("雲の画像")]
    public Sprite[] cloudSprites;
    public string resourcesFolder = "Clouds";
    public int sortingOrder = -9;

    [Header("配置")]
    public int cloudCount = 7;
    public float baseY = 3.15f;
    public float yVariation = 0.65f;
    public float spacing = 5.2f;
    public Vector2 scaleRange = new Vector2(0.75f, 1.25f);

    [Header("流れる量")]
    public float parallaxStrength = 0.12f;
    public float idleDriftSpeed = 0.03f;
    public int randomSeed = 23;

    private readonly List<Transform> clouds = new List<Transform>();
    private readonly List<float> yOffsets = new List<float>();
    private float lastPlayerX;
    private float scrollOffset;

    void Start()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

        if (cloudSprites == null || cloudSprites.Length == 0)
        {
            cloudSprites = Resources.LoadAll<Sprite>(resourcesFolder);
        }

        if (player != null)
        {
            lastPlayerX = player.position.x;
        }

        CreateClouds();
        PositionClouds();
    }

    void LateUpdate()
    {
        if (targetCamera == null || player == null || clouds.Count == 0)
        {
            return;
        }

        float playerDeltaX = player.position.x - lastPlayerX;
        lastPlayerX = player.position.x;

        scrollOffset -= playerDeltaX * parallaxStrength;
        scrollOffset -= idleDriftSpeed * Time.deltaTime;

        PositionClouds();
    }

    void CreateClouds()
    {
        if (cloudSprites == null || cloudSprites.Length == 0)
        {
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        for (int i = 0; i < cloudCount; i++)
        {
            GameObject cloud = new GameObject("FarCloud_" + (i + 1));
            cloud.transform.SetParent(transform);

            SpriteRenderer renderer = cloud.AddComponent<SpriteRenderer>();
            renderer.sprite = cloudSprites[i % cloudSprites.Length];
            renderer.sortingOrder = sortingOrder;
            renderer.flipX = Random.value > 0.5f;

            float scale = Random.Range(scaleRange.x, scaleRange.y);
            cloud.transform.localScale = Vector3.one * scale;

            yOffsets.Add(Random.Range(-yVariation, yVariation));
            clouds.Add(cloud.transform);
        }

        Random.state = previousState;
    }

    void PositionClouds()
    {
        float cameraX = targetCamera.transform.position.x;
        float totalWidth = spacing * cloudCount;
        scrollOffset = Mathf.Repeat(scrollOffset, totalWidth);
        float startX = cameraX - (totalWidth * 0.5f) + scrollOffset;

        for (int i = 0; i < clouds.Count; i++)
        {
            float x = startX + (spacing * i);

            if (x < cameraX - (totalWidth * 0.5f))
            {
                x += totalWidth;
            }
            else if (x > cameraX + (totalWidth * 0.5f))
            {
                x -= totalWidth;
            }

            clouds[i].position = new Vector3(x, baseY + yOffsets[i], 0f);
        }
    }
}
