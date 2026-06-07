using System.Collections.Generic;
using UnityEngine;

public class ForegroundTreeParallax : MonoBehaviour
{
    private const string GeneratedTreePrefix = "__GeneratedForegroundTree_";

    [Header("追いかける対象")]
    public Transform player;
    public Camera targetCamera;

    [Header("木の見た目")]
    public Sprite treeSprite;
    public int sortingOrder = 20;
    public float treeScale = 1.55f;
    public float baseY = -1.85f;

    [Header("流れる量")]
    public int treeCount = 5;
    public float gapInTreeWidths = 5f;
    public float minGapInTreeWidths = 3.5f;
    public float maxGapInTreeWidths = 6.5f;
    public float parallaxStrength = 1.25f;
    public float idleDriftSpeed = 0f;
    public int randomSeed = 71;

    private readonly List<Transform> trees = new List<Transform>();
    private float[] relativeXOffsets;
    private float lastPlayerX;
    private float scrollOffset;
    private float totalWidth;

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

        if (player != null)
        {
            lastPlayerX = player.position.x;
        }

        CreateTrees();
        PositionTrees();
    }

    void LateUpdate()
    {
        if (targetCamera == null || player == null || trees.Count == 0)
        {
            return;
        }

        float playerDeltaX = player.position.x - lastPlayerX;
        lastPlayerX = player.position.x;

        scrollOffset -= playerDeltaX * parallaxStrength;
        scrollOffset -= idleDriftSpeed * Time.deltaTime;

        PositionTrees();
    }

    void CreateTrees()
    {
        CleanupGeneratedTrees();

        if (treeSprite == null)
        {
            return;
        }

        int safeTreeCount = Mathf.Max(0, treeCount);
        relativeXOffsets = new float[safeTreeCount];
        totalWidth = 0f;

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        for (int i = 0; i < safeTreeCount; i++)
        {
            GameObject tree = new GameObject(GeneratedTreePrefix + (i + 1));
            tree.transform.SetParent(transform);
            tree.hideFlags = HideFlags.DontSave | HideFlags.HideInHierarchy;
            tree.transform.localScale = Vector3.one * Mathf.Max(0.01f, treeScale);

            SpriteRenderer renderer = tree.AddComponent<SpriteRenderer>();
            renderer.sprite = treeSprite;
            renderer.sortingOrder = sortingOrder;

            relativeXOffsets[i] = totalWidth;
            float minGap = Mathf.Max(0f, Mathf.Min(minGapInTreeWidths, maxGapInTreeWidths));
            float maxGap = Mathf.Max(0f, Mathf.Max(minGapInTreeWidths, maxGapInTreeWidths));
            float randomGapInWidths = Random.Range(minGap, maxGap);
            float treeWidth = treeSprite.bounds.size.x * Mathf.Max(0.01f, treeScale);
            totalWidth += treeWidth * (randomGapInWidths + 1f);

            trees.Add(tree.transform);
        }

        Random.state = previousState;
    }

    void CleanupGeneratedTrees()
    {
        trees.Clear();

        List<GameObject> childrenToDestroy = new List<GameObject>();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (child.name.StartsWith(GeneratedTreePrefix) || child.name.StartsWith("ForegroundTree_"))
            {
                childrenToDestroy.Add(child.gameObject);
            }
        }

        foreach (GameObject child in childrenToDestroy)
        {
            if (Application.isPlaying)
            {
                Destroy(child);
            }
            else
            {
                DestroyImmediate(child);
            }
        }
    }

    void PositionTrees()
    {
        float cameraX = targetCamera.transform.position.x;
        if (totalWidth <= 0f || relativeXOffsets == null)
        {
            return;
        }
        scrollOffset = Mathf.Repeat(scrollOffset, totalWidth);

        float startX = cameraX - (totalWidth * 0.5f) + scrollOffset;

        for (int i = 0; i < trees.Count; i++)
        {
            float x = startX + relativeXOffsets[i];

            if (x < cameraX - (totalWidth * 0.5f))
            {
                x += totalWidth;
            }
            else if (x > cameraX + (totalWidth * 0.5f))
            {
                x -= totalWidth;
            }

            trees[i].position = new Vector3(x, baseY, 0f);
        }
    }

    void OnDisable()
    {
        CleanupGeneratedTrees();
    }
}
