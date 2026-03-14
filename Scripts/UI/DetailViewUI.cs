using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class DetailViewUI : MonoBehaviour
{
    [Header("Detail View UI")]
    public GameObject detailPanel;
    public RawImage detailCameraImage;
    public Image detailStaticOverlay;
    public Button prevButton;
    public Button nextButton;
    public TextMeshProUGUI cameraIndexText;

    private int currentCameraIndex = 0;

    #region Lifecycle
    private void Start()
    {
        InitializeUI();
        detailPanel.SetActive(false);
    }
    private void OnEnable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged += HandleModeChanged;
            ViewManager.Instance.OnCameraSelected += HandleCameraSelected;
            ViewManager.Instance.OnStaticChanged += HandleStaticChanged;
        }
    }

    private void OnDisable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged -= HandleModeChanged;
            ViewManager.Instance.OnCameraSelected -= HandleCameraSelected;
            ViewManager.Instance.OnStaticChanged -= HandleStaticChanged;
        }
    }
    #endregion

    #region Initialization
    private void InitializeUI()
    {
        prevButton.onClick.AddListener(OnPreviousButtonClick);
        nextButton.onClick.AddListener(OnNextButtonClick);
    }
    #endregion

    #region Event Handlers
    private void HandleModeChanged(ViewMode mode)
    {
        if (mode == ViewMode.DetailView)
        {
            ShowDetailView();
        }
        else
        {
            HideDetailView();
        }
    }

    private void HandleCameraSelected(int cameraIndex)
    {
        currentCameraIndex = cameraIndex;
        UpdateDetailView();
    }

    private void HandleStaticChanged(int cameraIndex, bool hasStatic)
    {
        if (cameraIndex == currentCameraIndex)
        {
            UpdateStaticOverlay(hasStatic);
        }
    }
    #endregion

    #region UI Control
    private void ShowDetailView()
    {
        detailPanel.SetActive(true);
        UpdateDetailView();
    }

    private void HideDetailView()
    {
        detailPanel.SetActive(false);
    }

    private void UpdateDetailView()
    {
        if (ViewManager.Instance == null) return;

        CameraData data = ViewManager.Instance.GetCameraData(currentCameraIndex);
        if (data == null) return;

        detailCameraImage.texture = data.detailTexture;

        UpdateStaticOverlay(data.hasStatic);

        cameraIndexText.text = $"Cam {(currentCameraIndex + 1):00}";
    }

    private void UpdateStaticOverlay(bool hasStatic)
    {
        detailCameraImage.gameObject.SetActive(!hasStatic);
        detailStaticOverlay.gameObject.SetActive(hasStatic);
    }
    #endregion

    #region Button Callbacks
    private void OnPreviousButtonClick()
    {
        ViewManager.Instance.SwitchToPreviousCamera();
    }

    private void OnNextButtonClick()
    {
        ViewManager.Instance.SwitchToNextCamera();
    }
    #endregion

    #region Public Methods
    public void RefreshDetailView()
    {
        if (detailPanel.activeInHierarchy)
        {
            UpdateDetailView();
        }
    }

    public int GetCurrentCameraIndex()
    {
        return currentCameraIndex;
    }
    #endregion
}
