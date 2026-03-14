using UnityEngine;

public class CameraData
{
    public Camera camera;
    public RenderTexture gridTexture;
    public RenderTexture detailTexture;
    public bool hasStatic;
    public int index;

    public CameraData(Camera cam, int idx, Vector2 gridSize, Vector2 detailSize)
    {
        camera = cam;
        index = idx;
        hasStatic = false;

        gridTexture = new RenderTexture(
            (int)gridSize.x,
            (int)gridSize.y,
            16  
        );
        gridTexture.name = $"Camera_{idx}_Grid"; // ?

        detailTexture = new RenderTexture(
            (int)detailSize.x,
            (int)detailSize.y,
            16
        );
        detailTexture.name = $"Camera_{idx}_Detail"; // ?

        camera.targetTexture = gridTexture;
        camera.enabled = false;
    }

    public void Release()
    {
        if (gridTexture != null)
        {
            gridTexture.Release();
            gridTexture = null;
        }

        if (detailTexture != null)
        {
            detailTexture.Release();
            detailTexture = null;
        }
    }
}
