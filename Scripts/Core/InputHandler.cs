using UnityEngine;

/// <summary>
/// Handles all keyboard and mouse input for camera system navigation
/// Delegates commands to ViewManager without containing business logic
/// </summary>
public class InputHandler : MonoBehaviour
{
    private ViewManager viewManager;

    private void Start()
    {
        viewManager = ViewManager.Instance;

        if (viewManager == null)
        {
            Debug.LogError("ViewManager instance not found! InputHandler requires ViewManager.");
            enabled = false;
        }
    }

    private void Update()
    {
        HandleModeNavigation();

        if (viewManager.IsPaused()) return;

        HandleDetailViewNavigation();
        HandleFloorPlanNavigation();
    }

    private void HandleModeNavigation()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ViewMode currentMode = viewManager.GetCurrentMode();

            switch (currentMode)
            {
                case ViewMode.Panopticon:
                    viewManager.TogglePause();
                    break;

                case ViewMode.DetailView:
                    viewManager.SwitchToOverview();
                    break;

                case ViewMode.CameraOverview:
                case ViewMode.FloorPlan:
                    viewManager.SwitchToPanopticon();
                    break;
            }
        }
    }

    private void HandleDetailViewNavigation()
    {
        if (viewManager.GetCurrentMode() != ViewMode.DetailView) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            viewManager.SwitchToPreviousCamera();
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            viewManager.SwitchToNextCamera();
        }
    }

    private void HandleFloorPlanNavigation()
    {
        if (viewManager.GetCurrentMode() != ViewMode.FloorPlan) return;

        if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (viewManager.blueprintUI != null)
            {
                viewManager.blueprintUI.SwitchToPreviousFloor();
            }
        }

        if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (viewManager.blueprintUI != null)
            {
                viewManager.blueprintUI.SwitchToNextFloor();
            }
        }
    }
}