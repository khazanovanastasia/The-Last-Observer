using UnityEngine;
using UnityEngine.UI;

public class UIDrawSurface : MonoBehaviour
{
    public int textureSize = 1024;
    public Color drawColor = Color.black;
    public int brushSize = 3;
    public int eraseSize = 8;

    private Texture2D drawTexture;
    private RawImage rawImage;
    private RectTransform rectTransform;

    private Vector2 previousPixelPos;
    private bool isDrawing;

    #region Lifecycle
    private void Awake()
    {
        rawImage = GetComponent<RawImage>();
        rectTransform = GetComponent<RectTransform>();

        CreateNewTexture();
    }

    private void Update()
    {
        HandleDrawingInput();
    }
    #endregion

    #region Input Handling
    private void HandleDrawingInput()
    {
        if (Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
        {
            isDrawing = TryGetPixelPosition(out previousPixelPos);
        }

        if (Input.GetMouseButton(0))
        {
            Draw(drawColor, brushSize);
        }

        if (Input.GetMouseButton(1))
        {
            Draw(Color.clear, eraseSize);
        }

        if (Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1))
        {
            isDrawing = false;
        }
    }
    #endregion

    #region Drawing
    void Draw(Color color, int size)
    {
        if (!isDrawing) return;

        if (!TryGetPixelPosition(out Vector2 pixelPos))
            return;

        DrawLine(previousPixelPos, pixelPos, color, size);
        previousPixelPos = pixelPos;

        drawTexture.Apply();
    }

    bool TryGetPixelPosition(out Vector2 pixelPos)
    {
        pixelPos = Vector2.zero;

        if (!RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            Input.mousePosition))
            return false;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rectTransform,
            Input.mousePosition,
            null,
            out Vector2 localPoint);

        Rect rect = rectTransform.rect;

        float x = (localPoint.x - rect.x) / rect.width;
        float y = (localPoint.y - rect.y) / rect.height;

        pixelPos.x = x * textureSize;
        pixelPos.y = y * textureSize;

        return true;
    }

    void DrawLine(Vector2 from, Vector2 to, Color color, int size)
    {
        float distance = Vector2.Distance(from, to);
        int steps = Mathf.CeilToInt(distance);

        for (int i = 0; i < steps; i++)
        {
            Vector2 pos = Vector2.Lerp(from, to, i / (float)steps);
            DrawCircle((int)pos.x, (int)pos.y, size, color);
        }
    }

    void DrawCircle(int cx, int cy, int radius, Color color)
    {
        for (int x = -radius; x <= radius; x++)
        {
            for (int y = -radius; y <= radius; y++)
            {
                if (x * x + y * y > radius * radius)
                    continue;

                int px = cx + x;
                int py = cy + y;

                if (px < 0 || px >= textureSize || py < 0 || py >= textureSize)
                    continue;

                drawTexture.SetPixel(px, py, color);
            }
        }
    }
    #endregion

    #region Texture Management
    private void CreateNewTexture()
    {
        drawTexture = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        drawTexture.filterMode = FilterMode.Bilinear;

        ClearTexture();

        rawImage.texture = drawTexture;
    }

    private void ClearTexture()
    {
        Color clear = new Color(0, 0, 0, 0);
        Color[] pixels = new Color[textureSize * textureSize];

        for (int i = 0; i < pixels.Length; i++)
        {
            pixels[i] = clear;
        }

        drawTexture.SetPixels(pixels);
        drawTexture.Apply();
    }

    public void SetTexture(Texture2D tex)
    {
        if (tex == null)
        {
            Debug.LogWarning("UIDrawSurface: Attempted to set null texture");
            return;
        }

        drawTexture = tex;
        rawImage.texture = drawTexture;

        textureSize = tex.width;
    }

    public Texture2D GetTexture()
    {
        return drawTexture;
    }
    #endregion
}
