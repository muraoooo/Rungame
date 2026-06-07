using System.Collections.Generic;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

[ExecuteAlways]
public class EnvironmentParallaxLoop : MonoBehaviour
{
    private const string GeneratedElementPrefix = "__GeneratedEnvironmentParallaxElement_";

    [Header("Target to follow")]
    public Transform player;
    public Camera targetCamera;

    [Header("Sprites")]
    public Sprite[] sprites;
    public string resourcesFolder = "";
    public int sortingOrder = -9;
    public Color tint = Color.white;
    public bool hideGeneratedElementsInHierarchy;

    [Header("Placement")]
    public int elementCount = 7;
    public float baseY = 3.15f;
    public float yVariation = 0.65f;
    public float minSpacing = 4f;
    public float maxSpacing = 7f;
    public Vector2 scaleRange = new Vector2(0.75f, 1.25f);

    [Header("Parallax Settings")]
    public float parallaxStrength = 0.12f;
    public float idleDriftSpeed = 0.03f;
    public int randomSeed = 23;

    private List<Transform> elements = new List<Transform>();
    private SpriteRenderer[] elementRenderers;
    private float[] relativeXOffsets;
    private float[] individualYOffsets;
    private float totalWidth;
    private float lastPlayerX;
    private float scrollOffset;
    private bool hasInitialized;
#if UNITY_EDITOR
    private bool editorRefreshQueued;
#endif

    void OnEnable()
    {
        Initialize();
    }

    void Start()
    {
        if (!hasInitialized)
        {
            Initialize();
        }
    }

    void Initialize()
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

        if ((sprites == null || sprites.Length == 0) && !string.IsNullOrEmpty(resourcesFolder))
        {
            sprites = Resources.LoadAll<Sprite>(resourcesFolder);
        }

        if (player != null)
        {
            lastPlayerX = player.position.x;
        }

        CreateElements();
        PositionElements();
        hasInitialized = true;
    }

    void LateUpdate()
    {
        if (targetCamera == null || player == null || elements.Count == 0)
        {
            // In Edit Mode, we might not have a player or camera active in the same way,
            // but we still want to see the elements at their base positions.
            if (!Application.isPlaying)
            {
                PositionElements();
            }
            return;
        }

        float playerDeltaX = player.position.x - lastPlayerX;
        lastPlayerX = player.position.x;

        if (Application.isPlaying)
        {
            scrollOffset -= playerDeltaX * parallaxStrength;
            scrollOffset -= idleDriftSpeed * Time.deltaTime;
        }

        PositionElements();
    }

    void CreateElements()
    {
        CleanupGeneratedElements();

        if (sprites == null || sprites.Length == 0)
        {
            return;
        }

        int safeElementCount = Mathf.Max(0, elementCount);
        if (safeElementCount == 0)
        {
            return;
        }

        Random.State previousState = Random.state;
        Random.InitState(randomSeed);

        relativeXOffsets = new float[safeElementCount];
        individualYOffsets = new float[safeElementCount];
        elementRenderers = new SpriteRenderer[safeElementCount];
        totalWidth = 0f;

        for (int i = 0; i < safeElementCount; i++)
        {
            GameObject element = new GameObject(GeneratedElementPrefix + (i + 1));
            element.transform.SetParent(transform);
            element.hideFlags = hideGeneratedElementsInHierarchy
                ? HideFlags.DontSave | HideFlags.HideInHierarchy
                : HideFlags.DontSave;

            SpriteRenderer renderer = element.AddComponent<SpriteRenderer>();
            renderer.sprite = sprites[Random.Range(0, sprites.Length)];
            renderer.sortingOrder = sortingOrder;
            renderer.color = tint;
            renderer.flipX = Random.value > 0.5f;
            elementRenderers[i] = renderer;

            float scale = Random.Range(Mathf.Min(scaleRange.x, scaleRange.y), Mathf.Max(scaleRange.x, scaleRange.y));
            element.transform.localScale = Vector3.one * scale;

            float safeYVariation = Mathf.Abs(yVariation);
            individualYOffsets[i] = Random.Range(-safeYVariation, safeYVariation);
            
            // Randomize spacing
            relativeXOffsets[i] = totalWidth;
            float minSafeSpacing = Mathf.Max(0.01f, Mathf.Min(minSpacing, maxSpacing));
            float maxSafeSpacing = Mathf.Max(0.01f, Mathf.Max(minSpacing, maxSpacing));
            float currentSpacing = Random.Range(minSafeSpacing, maxSafeSpacing);
            totalWidth += currentSpacing;

            elements.Add(element.transform);
        }

        Random.state = previousState;
    }

    void CleanupGeneratedElements()
    {
        elements.Clear();

        List<GameObject> childrenToDestroy = new List<GameObject>();
        for (int i = transform.childCount - 1; i >= 0; i--)
        {
            Transform child = transform.GetChild(i);
            if (IsGeneratedElement(child))
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

    bool IsGeneratedElement(Transform child)
    {
        return child.name.StartsWith(GeneratedElementPrefix)
            || child.name.StartsWith(gameObject.name + "_Element_")
            || child.name.StartsWith(gameObject.name + "_Elem")
            || (child.childCount == 0 && child.GetComponent<SpriteRenderer>() != null);
    }

    void PositionElements()
    {
        if (elements.Count == 0 || totalWidth <= 0f) return;

        float cameraX = (targetCamera != null) ? targetCamera.transform.position.x : 0f;
        
        // Loop the scroll offset within totalWidth
        float currentScroll = Mathf.Repeat(scrollOffset, totalWidth);
        
        // startX relative to camera to keep elements centered around camera view
        float startX = cameraX - (totalWidth * 0.5f) + currentScroll;

        for (int i = 0; i < elements.Count; i++)
        {
            if (elements[i] == null) continue;

            float x = startX + relativeXOffsets[i];

            // Wrap around the total width relative to camera view
            if (x < cameraX - (totalWidth * 0.5f))
            {
                x += totalWidth;
            }
            else if (x > cameraX + (totalWidth * 0.5f))
            {
                x -= totalWidth;
            }

            elements[i].position = new Vector3(
                x,
                transform.position.y + baseY + individualYOffsets[i],
                transform.position.z
            );
        }
    }

    void RandomizeElement(int index)
    {
        if (index < 0 || index >= elements.Count || elements[index] == null)
        {
            return;
        }

        float safeYVariation = Mathf.Abs(yVariation);
        individualYOffsets[index] = Random.Range(-safeYVariation, safeYVariation);
        float scale = Random.Range(Mathf.Min(scaleRange.x, scaleRange.y), Mathf.Max(scaleRange.x, scaleRange.y));
        elements[index].localScale = Vector3.one * scale;
        if (elementRenderers[index] != null && sprites.Length > 0)
        {
            elementRenderers[index].sprite = sprites[Random.Range(0, sprites.Length)];
            elementRenderers[index].color = tint;
            elementRenderers[index].flipX = Random.value > 0.5f;
        }
    }

    void OnValidate()
    {
#if UNITY_EDITOR
        if (Application.isPlaying || editorRefreshQueued)
        {
            return;
        }

        editorRefreshQueued = true;
        EditorApplication.delayCall += RefreshInEditor;
#endif
    }

#if UNITY_EDITOR
    void RefreshInEditor()
    {
        editorRefreshQueued = false;
        if (this == null || Application.isPlaying || !gameObject.activeInHierarchy)
        {
            return;
        }

        Initialize();
    }
#endif

    void OnDisable()
    {
        CleanupGeneratedElements();
        hasInitialized = false;
    }
}
