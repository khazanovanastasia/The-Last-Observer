using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CameraGridUI : MonoBehaviour
{
    [Header("Grid Settings")]
    public GameObject gridPanel;
    public ScrollRect scrollRect;
    public GridLayoutGroup gridLayoutGroup;
    public GameObject cameraButtonPrefab;

    [Header("Camera Settings")]
    public int gridColumn = 4;
    public Vector2 imageSize = new Vector2(340, 256);

    private List<CameraButton> cameraButtons = new List<CameraButton>();

    #region Lifecycle
    private void Start()
    {
        InitializeCameraGrid();
        gridPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged += HandleModeChanged;
            ViewManager.Instance.OnStaticChanged += HandleStaticChanged;
        }
    }

    private void OnDisable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged -= HandleModeChanged;
            ViewManager.Instance.OnStaticChanged -= HandleStaticChanged;
        }
    }

    private void Update()
    {
        if (gridPanel.activeInHierarchy)
        {
            HandleScrollInput();
        }
    }
    #endregion

    #region Initialization
    private void InitializeCameraGrid()
    {
        gridLayoutGroup.cellSize = imageSize;
        gridLayoutGroup.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridLayoutGroup.constraintCount = gridColumn;

        int cameraCount = ViewManager.Instance.GetCameraCount();

        int rowCount = Mathf.CeilToInt((float)cameraCount / gridColumn);
        scrollRect.content.sizeDelta = new Vector2(
            scrollRect.content.sizeDelta.x,
            (imageSize.y + gridLayoutGroup.spacing.y) * rowCount
        );

        for (int i = 0; i < cameraCount; i++)
        {
            CreateCameraButton(i);
        }
        ApplyInitialStaticEffects();
    }

    private void CreateCameraButton(int cameraIndex)
    {
        GameObject buttonObj = Instantiate(cameraButtonPrefab, gridLayoutGroup.transform);
        CameraButton button = buttonObj.GetComponent<CameraButton>();

        RenderTexture gridTexture = ViewManager.Instance.GetGridTexture(cameraIndex);
        button.Initialize(cameraIndex, gridTexture, OnCameraButtonClick);

        cameraButtons.Add(button);
    }
    private void ApplyInitialStaticEffects()
    {
        for (int i = 0; i < cameraButtons.Count; i++)
        {
            CameraData data = ViewManager.Instance.GetCameraData(i);
            if (data != null && data.hasStatic)
            {
                UpdateCameraStaticEffect(i, true);
            }
        }
    }
    #endregion

    #region Event Handlers
    private void HandleModeChanged(ViewMode mode)
    {
        if (mode == ViewMode.CameraOverview)
        {
            ShowGrid();
        }
        else
        {
            HideGrid();
        }
    }

    private void HandleStaticChanged(int cameraIndex, bool hasStatic)
    {
        UpdateCameraStaticEffect(cameraIndex, hasStatic);
    }

    private void OnCameraButtonClick(int cameraIndex)
    {
        ViewManager.Instance.SwitchToDetailView(cameraIndex);
    }
    #endregion

    #region UI Control
    private void ShowGrid()
    {
        gridPanel.SetActive(true);
        RefreshCameraTextures();
    }

    private void HideGrid()
    {
        gridPanel.SetActive(false);
    }

    private void RefreshCameraTextures()
    {
        for (int i = 0; i < cameraButtons.Count; i++)
        {
            RenderTexture gridTexture = ViewManager.Instance.GetGridTexture(i);
            cameraButtons[i].UpdateTexture(gridTexture);
        }
    }

    private void UpdateCameraStaticEffect(int cameraIndex, bool hasStatic)
    {
        if (cameraIndex >= 0 && cameraIndex < cameraButtons.Count)
        {
            CameraStaticEffect staticEffect = cameraButtons[cameraIndex].GetComponent<CameraStaticEffect>();
            if (staticEffect != null)
            {
                staticEffect.ShowStatic(hasStatic);
            }
        }
    }
    #endregion

    #region Input Handling
    private void HandleScrollInput()
    {
        float scroll = Input.GetAxis("Mouse ScrollWheel");
        if (scroll != 0)
        {
            scrollRect.verticalNormalizedPosition += scroll * 0.1f;
            scrollRect.verticalNormalizedPosition = Mathf.Clamp01(scrollRect.verticalNormalizedPosition);
        }
    }
    #endregion

    #region Public Getters
    public List<CameraButton> GetCameraButtons()
    {
        return cameraButtons;
    }
    #endregion
}


