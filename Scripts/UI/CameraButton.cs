using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CameraButton : MonoBehaviour
{
    [Header("UI Components")]
    public Button button;
    public RawImage cameraRawImage;
    public TextMeshProUGUI cameraIndexText;

    private int cameraIndex;
    private System.Action<int> onClickCallback;

    public void Initialize(int index, RenderTexture renderTexture, System.Action<int> onClick)
    {
        cameraIndex = index;
        onClickCallback = onClick;

        cameraRawImage.texture = renderTexture;
        cameraIndexText.text = "Cam " + (index + 1).ToString("00");

        button.onClick.AddListener(OnButtonClick);
    }

    public void UpdateTexture(RenderTexture renderTexture)
    {
        cameraRawImage.texture = renderTexture;
    }

    private void OnButtonClick()
    {
        onClickCallback?.Invoke(cameraIndex);
    }

    public int GetCameraIndex()
    {
        return cameraIndex;
    }

}
