using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class FloorPlanUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject blueprintPanel;
    public UIDrawSurface drawSurface;
    public Button prevFloorButton; 
    public Button nextFloorButton; 

    [Header("Floor Settings")]
    public Texture2D[] baseFloorTextures; 
    public int startingFloor = 0;

    private List<Texture2D> currentFloorTextures; 
    private int currentFloorIndex = 0;

    #region Lifecycle
    private void Start()
    {
        InitializeFloors();
        InitializeButtons();
        blueprintPanel.SetActive(false);
    }

    private void OnEnable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged += HandleModeChanged;
        }
    }

    private void OnDisable()
    {
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged -= HandleModeChanged;
        }
    }
    #endregion

    #region Initialization
    private void InitializeFloors()
    {
        if (baseFloorTextures == null || baseFloorTextures.Length == 0)
        {
            Debug.LogError("FloorPlanUI: No base floor textures assigned!");
            return;
        }

        currentFloorTextures = new List<Texture2D>();

        for (int i = 0; i < baseFloorTextures.Length; i++)
        {
            Texture2D newTexture = DuplicateTexture(baseFloorTextures[i]);
            currentFloorTextures.Add(newTexture);
        }

        currentFloorIndex = Mathf.Clamp(startingFloor, 0, currentFloorTextures.Count - 1);
    }

    private void InitializeButtons()
    {
        if (prevFloorButton != null)
        {
            prevFloorButton.onClick.AddListener(SwitchToPreviousFloor);
        }

        if (nextFloorButton != null)
        {
            nextFloorButton.onClick.AddListener(SwitchToNextFloor);
        }
    }
    #endregion

    #region Event Handlers
    private void HandleModeChanged(ViewMode mode)
    {
        if (mode == ViewMode.FloorPlan)
        {
            ShowFloorPlanView();
        }
        else
        {
            HideFloorPlanView();
        }
    }
    #endregion

    #region UI Control
    public void ShowFloorPlanView()
    {
        blueprintPanel.SetActive(true);

        if (drawSurface != null && currentFloorTextures.Count > 0)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        UpdateFloorIndicator();
    }

    public void HideFloorPlanView()
    {
        SaveCurrentFloor();

        blueprintPanel.SetActive(false);
    }

    private void UpdateFloorIndicator()
    {
        if (prevFloorButton != null)
        {
            prevFloorButton.interactable = currentFloorTextures.Count > 1;
        }

        if (nextFloorButton != null)
        {
            nextFloorButton.interactable = currentFloorTextures.Count > 1;
        }
    }
    #endregion

    #region Floor Navigation
    public void SwitchToPreviousFloor()
    {
        if (currentFloorTextures.Count <= 1) return;

        int targetFloor = currentFloorIndex - 1;

        if (targetFloor < 0)
        {
            targetFloor = currentFloorTextures.Count - 1;
        }

        SwitchFloor(targetFloor);
    }

    public void SwitchToNextFloor()
    {
        if (currentFloorTextures.Count <= 1) return;

        int targetFloor = currentFloorIndex + 1;

        if (targetFloor >= currentFloorTextures.Count)
        {
            targetFloor = 0;
        }

        SwitchFloor(targetFloor);
    }

    public void SwitchFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= currentFloorTextures.Count)
        {
            Debug.LogWarning($"Invalid floor index: {floorIndex}");
            return;
        }

        SaveCurrentFloor();

        currentFloorIndex = floorIndex;

        if (drawSurface != null)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        UpdateFloorIndicator();
    }
    #endregion

    #region Memory Management
    private void SaveCurrentFloor()
    {
        if (drawSurface == null || currentFloorTextures.Count == 0) return;

        Texture2D currentTexture = drawSurface.GetTexture();

        currentFloorTextures[currentFloorIndex] = currentTexture;
    }

    private Texture2D DuplicateTexture(Texture2D source)
    {
        if (source == null)
        {
            Debug.LogError("Cannot duplicate null texture!");
            return null;
        }

        RenderTexture rt = RenderTexture.GetTemporary(source.width, source.height);
        Graphics.Blit(source, rt);

        RenderTexture previous = RenderTexture.active;
        RenderTexture.active = rt;

        Texture2D copy = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
        copy.ReadPixels(new Rect(0, 0, rt.width, rt.height), 0, 0);
        copy.Apply();

        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(rt);

        return copy;
    }
    #endregion

    #region Public Methods
    public void ClearAllNotes()
    {
        currentFloorTextures.Clear();

        for (int i = 0; i < baseFloorTextures.Length; i++)
        {
            Texture2D newTexture = DuplicateTexture(baseFloorTextures[i]);
            currentFloorTextures.Add(newTexture);
        }

        if (blueprintPanel.activeSelf && drawSurface != null)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        Debug.Log("All floor plan notes cleared");
    }

    public void ClearCurrentFloorNotes()
    {
        Texture2D cleanTexture = DuplicateTexture(baseFloorTextures[currentFloorIndex]);
        currentFloorTextures[currentFloorIndex] = cleanTexture;

        if (drawSurface != null)
        {
            drawSurface.SetTexture(cleanTexture);
        }

        Debug.Log($"Floor {currentFloorIndex + 1} notes cleared");
    }

    public int GetCurrentFloorIndex()
    {
        return currentFloorIndex;
    }

    public int GetFloorCount()
    {
        return currentFloorTextures.Count;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        if (blueprintPanel.activeSelf)
        {
            SaveCurrentFloor();
        }
    }
    #endregion
}