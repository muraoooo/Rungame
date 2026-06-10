using System.Collections.Generic;
using UnityEngine;

public class StageArtTiledLayer : MonoBehaviour
{
    public Sprite sprite;
    public int sortingOrder;
    public float baseY;
    public float scale = 1f;
    public float parallaxStrength;
    public float heightScale = 1f;

    readonly List<Transform> tiles = new List<Transform>();
    Camera targetCamera;
    Transform player;
    float lastPlayerX;
    float scrollOffset;
    float tileWidth;

    void Start()
    {
        targetCamera = Camera.main;

        GameObject playerObject = GameObject.FindGameObjectWithTag("Player");
        if (playerObject != null)
        {
            player = playerObject.transform;
            lastPlayerX = player.position.x;
        }

        CreateTiles();
        PositionTiles();
    }

    void LateUpdate()
    {
        if (targetCamera == null)
        {
            targetCamera = Camera.main;
        }

        if (targetCamera == null || tiles.Count == 0)
        {
            return;
        }

        if (player != null)
        {
            float deltaX = player.position.x - lastPlayerX;
            lastPlayerX = player.position.x;
            scrollOffset -= deltaX * parallaxStrength;
        }

        PositionTiles();
    }

    void CreateTiles()
    {
        if (sprite == null)
        {
            return;
        }

        float cameraHeight = targetCamera != null ? targetCamera.orthographicSize * 2f : 10f;
        float spriteHeight = Mathf.Max(0.01f, sprite.bounds.size.y);
        float fitScale = cameraHeight / spriteHeight * heightScale * Mathf.Max(0.01f, scale);
        tileWidth = sprite.bounds.size.x * fitScale;

        for (int i = 0; i < 5; i++)
        {
            GameObject tile = new GameObject("Tile_" + (i + 1));
            tile.transform.SetParent(transform);
            tile.transform.localScale = Vector3.one * fitScale;

            SpriteRenderer renderer = tile.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.sortingOrder = sortingOrder;

            tiles.Add(tile.transform);
        }
    }

    void PositionTiles()
    {
        if (targetCamera == null || tileWidth <= 0f)
        {
            return;
        }

        float cameraX = targetCamera.transform.position.x;
        float totalWidth = tileWidth * tiles.Count;
        scrollOffset = Mathf.Repeat(scrollOffset, tileWidth);
        float startX = cameraX - tileWidth * 2f + scrollOffset;

        for (int i = 0; i < tiles.Count; i++)
        {
            float x = startX + tileWidth * i;
            while (x < cameraX - totalWidth * 0.5f)
            {
                x += totalWidth;
            }
            while (x > cameraX + totalWidth * 0.5f)
            {
                x -= totalWidth;
            }

            tiles[i].position = new Vector3(x, baseY, 0f);
        }
    }
}
