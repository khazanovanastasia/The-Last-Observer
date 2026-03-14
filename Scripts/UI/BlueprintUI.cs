using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlueprintUI : MonoBehaviour
{
    [Header("UI References")]
    public GameObject blueprintPanel;
    public UIDrawSurface drawSurface;
    public Button prevFloorButton; // Optional: UI button for previous floor
    public Button nextFloorButton; // Optional: UI button for next floor

    [Header("Floor Settings")]
    public Texture2D[] baseFloorTextures; // Default textures for each floor
    public int startingFloor = 0; // Which floor to show first (0-based)

    private List<Texture2D> currentFloorTextures; // Textures with player's drawings (in-memory only)
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
        // Subscribe to ViewManager events
        if (ViewManager.Instance != null)
        {
            ViewManager.Instance.OnModeChanged += HandleModeChanged;
        }
    }

    private void OnDisable()
    {
        // Unsubscribe to prevent memory leaks
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

        // Create editable copies of base textures
        for (int i = 0; i < baseFloorTextures.Length; i++)
        {
            Texture2D newTexture = DuplicateTexture(baseFloorTextures[i]);
            currentFloorTextures.Add(newTexture);
        }

        // Set starting floor
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

        // Load current floor texture into draw surface
        if (drawSurface != null && currentFloorTextures.Count > 0)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        UpdateFloorIndicator();
    }

    public void HideFloorPlanView()
    {
        // Save current floor before hiding
        SaveCurrentFloor();

        blueprintPanel.SetActive(false);
    }

    private void UpdateFloorIndicator()
    {
        // Buttons are always interactable since we wrap around
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
    /// <summary>
    /// Switch to previous floor (wraps to last floor if at first)
    /// Called by InputHandler or button
    /// </summary>
    public void SwitchToPreviousFloor()
    {
        if (currentFloorTextures.Count <= 1) return;

        int targetFloor = currentFloorIndex - 1;

        // Wrap around to last floor if at first
        if (targetFloor < 0)
        {
            targetFloor = currentFloorTextures.Count - 1;
        }

        SwitchFloor(targetFloor);
    }

    /// <summary>
    /// Switch to next floor (wraps to first floor if at last)
    /// Called by InputHandler or button
    /// </summary>
    public void SwitchToNextFloor()
    {
        if (currentFloorTextures.Count <= 1) return;

        int targetFloor = currentFloorIndex + 1;

        // Wrap around to first floor if at last
        if (targetFloor >= currentFloorTextures.Count)
        {
            targetFloor = 0;
        }

        SwitchFloor(targetFloor);
    }

    /// <summary>
    /// Switch to specific floor by index
    /// </summary>
    public void SwitchFloor(int floorIndex)
    {
        if (floorIndex < 0 || floorIndex >= currentFloorTextures.Count)
        {
            Debug.LogWarning($"Invalid floor index: {floorIndex}");
            return;
        }

        // Save current floor's texture with drawings
        SaveCurrentFloor();

        // Switch to new floor
        currentFloorIndex = floorIndex;

        // Load new floor's texture
        if (drawSurface != null)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        UpdateFloorIndicator();
    }
    #endregion

    #region Memory Management
    /// <summary>
    /// Save current floor texture from draw surface to memory
    /// No PlayerPrefs - drawings only persist during play session
    /// </summary>
    private void SaveCurrentFloor()
    {
        if (drawSurface == null || currentFloorTextures.Count == 0) return;

        // Get current texture from draw surface (includes player's notes)
        Texture2D currentTexture = drawSurface.GetTexture();

        // Update our cached texture in memory
        currentFloorTextures[currentFloorIndex] = currentTexture;
    }

    /// <summary>
    /// Create an editable copy of a texture
    /// </summary>
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
    /// <summary>
    /// Clear all notes from all floors (reset to base textures)
    /// Useful for new game or "Clear Notes" button
    /// </summary>
    public void ClearAllNotes()
    {
        // Recreate all floor textures from base textures
        currentFloorTextures.Clear();

        for (int i = 0; i < baseFloorTextures.Length; i++)
        {
            Texture2D newTexture = DuplicateTexture(baseFloorTextures[i]);
            currentFloorTextures.Add(newTexture);
        }

        // Refresh current floor display
        if (blueprintPanel.activeSelf && drawSurface != null)
        {
            drawSurface.SetTexture(currentFloorTextures[currentFloorIndex]);
        }

        Debug.Log("All floor plan notes cleared");
    }

    /// <summary>
    /// Clear notes from current floor only
    /// </summary>
    public void ClearCurrentFloorNotes()
    {
        // Reset to base texture
        Texture2D cleanTexture = DuplicateTexture(baseFloorTextures[currentFloorIndex]);
        currentFloorTextures[currentFloorIndex] = cleanTexture;

        // Update draw surface
        if (drawSurface != null)
        {
            drawSurface.SetTexture(cleanTexture);
        }

        Debug.Log($"Floor {currentFloorIndex + 1} notes cleared");
    }

    /// <summary>
    /// Get current floor index (0-based)
    /// </summary>
    public int GetCurrentFloorIndex()
    {
        return currentFloorIndex;
    }

    /// <summary>
    /// Get total number of floors
    /// </summary>
    public int GetFloorCount()
    {
        return currentFloorTextures.Count;
    }
    #endregion

    #region Cleanup
    private void OnDestroy()
    {
        // Save current floor before destroying
        if (blueprintPanel.activeSelf)
        {
            SaveCurrentFloor();
        }

        // Note: Textures will be garbage collected
        // No need for manual cleanup since we're not using PlayerPrefs
    }
    #endregion
}