using UnityEngine;
using UnityEngine.UI;

public static class EndBannerUI
{
    const float MaxBannerWidth = 1000f;
    const float MaxBannerHeight = 320f;

    public static bool Show(string resourcePath)
    {
        Sprite sprite = Resources.Load<Sprite>(resourcePath);
        if (sprite == null)
        {
            Debug.LogWarning("End banner image was not found: " + resourcePath);
            return false;
        }

        Canvas canvas = GetOrCreateCanvas();
        ClearPreviousBanner(canvas.transform);

        GameObject bannerObject = new GameObject(
            "EndBannerImage",
            typeof(RectTransform),
            typeof(CanvasRenderer),
            typeof(Image)
        );
        bannerObject.transform.SetParent(canvas.transform, false);

        Image image = bannerObject.GetComponent<Image>();
        image.sprite = sprite;
        image.preserveAspect = true;
        image.raycastTarget = false;

        RectTransform rectTransform = bannerObject.GetComponent<RectTransform>();
        rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.pivot = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = Vector2.zero;
        rectTransform.sizeDelta = GetBannerSize(sprite);

        return true;
    }

    public static void Clear()
    {
        GameObject canvasObject = GameObject.Find("EndBannerCanvas");
        if (canvasObject == null)
        {
            return;
        }

        ClearPreviousBanner(canvasObject.transform);
    }

    static Canvas GetOrCreateCanvas()
    {
        GameObject canvasObject = GameObject.Find("EndBannerCanvas");
        if (canvasObject != null)
        {
            return canvasObject.GetComponent<Canvas>();
        }

        canvasObject = new GameObject("EndBannerCanvas", typeof(RectTransform));

        Canvas canvas = canvasObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 100;

        CanvasScaler scaler = canvasObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1920f, 1080f);
        scaler.matchWidthOrHeight = 0.5f;

        canvasObject.AddComponent<GraphicRaycaster>();

        return canvas;
    }

    static void ClearPreviousBanner(Transform canvasTransform)
    {
        for (int i = canvasTransform.childCount - 1; i >= 0; i--)
        {
            Object.Destroy(canvasTransform.GetChild(i).gameObject);
        }
    }

    static Vector2 GetBannerSize(Sprite sprite)
    {
        float aspect = sprite.rect.width / sprite.rect.height;
        float width = MaxBannerWidth;
        float height = width / aspect;

        if (height > MaxBannerHeight)
        {
            height = MaxBannerHeight;
            width = height * aspect;
        }

        return new Vector2(width, height);
    }
}
