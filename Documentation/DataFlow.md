# Data Flow & Event Propagation

**Step-by-step examples of how data moves through the surveillance system**

---

## Table of Contents

1. [Event Flow Overview](#event-flow-overview)
2. [Scenario 1: Opening Camera Grid](#scenario-1-opening-camera-grid)
3. [Scenario 2: Switching to Detail View](#scenario-2-switching-to-detail-view)
4. [Scenario 3: Camera Navigation in Detail View](#scenario-3-camera-navigation-in-detail-view)
5. [Scenario 4: Randomized Camera Failures](#scenario-4-randomized-camera-failures)
6. [Scenario 5: Floor Plan Interaction](#scenario-5-floor-plan-interaction)
7. [Scenario 6: Returning to Panopticon](#scenario-6-returning-to-panopticon)

---

## Event Flow Overview

### Core Event Types

```csharp
// ViewManager events
public event Action<ViewMode> OnModeChanged;
public event Action<int, bool> OnStaticChanged;  // (cameraIndex, hasStatic)
public event Action<int> OnCameraSelected;       // (cameraIndex)
```

### Subscription Pattern

All UI components follow this pattern:

```csharp
void OnEnable() {
    ViewManager.Instance.OnModeChanged += HandleModeChanged;
    // ... other subscriptions
}

void OnDisable() {
    if (ViewManager.Instance != null) {
        ViewManager.Instance.OnModeChanged -= HandleModeChanged;
        // ... cleanup
    }
}
```

---

## Scenario 1: Opening Camera Grid

**User Action:** Player looks at monitor object in 3D scene and clicks

### Step-by-Step Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INPUT LAYER                                              │
└─────────────────────────────────────────────────────────────┘

Player clicks on monitor (3D object)
    ↓
PlayerInteraction.Update()
    ├─ Raycast hits InteractableObject (monitor)
    ├─ InteractableObject.interactionType == OpenCameraGrid
    └─ Calls: InteractableObject.Interact()

┌─────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER LAYER                                         │
└─────────────────────────────────────────────────────────────┘

InteractableObject.Interact()
    ↓
    Calls: ViewManager.SwitchToOverview()
        │
        ├─ currentMode = ViewMode.CameraOverview
        │
        ├─ SwitchToGridTextures()
        │   └─ Assigns gridRenderTexture to each camera.targetTexture
        │
        ├─ EnableAllCameras()
        │   └─ Sets camera.enabled = true for all 30 cameras
        │
        ├─ StartGridViewUpdates()
        │   └─ Starts coroutine: UpdateGridTexturesCoroutine()
        │       └─ Renders all cameras every 0.1 seconds
        │
        ├─ firstPersonLook.enabled = false
        │
        ├─ Cursor.visible = true
        │
        └─ Fires: OnModeChanged(ViewMode.CameraOverview)

┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW LAYER                                               │
└─────────────────────────────────────────────────────────────┘

CameraGridUI receives OnModeChanged(CameraOverview)
    ↓
    CameraGridUI.HandleModeChanged(CameraOverview)
        ├─ gridPanel.SetActive(true)
        └─ RefreshCameraTextures()
            └─ For each button:
                cameraButtons[i].UpdateTexture(
                    ViewManager.Instance.GetGridTexture(i)
                )

DetailViewUI receives OnModeChanged(CameraOverview)
    ↓
    DetailViewUI.HandleModeChanged(CameraOverview)
        └─ detailPanel.SetActive(false)  // Hide detail view

FloorPlanUI receives OnModeChanged(CameraOverview)
    ↓
    FloorPlanUI.HandleModeChanged(CameraOverview)
        └─ floorPlanPanel.SetActive(false)  // Hide floor plan

┌─────────────────────────────────────────────────────────────┐
│ 4. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ Grid panel visible with 30 camera feeds
✅ Cameras updating at 10 FPS (every 0.1s)
✅ Player cursor visible and unlocked
✅ First-person look disabled
✅ Other UI panels hidden
```

---

## Scenario 2: Switching to Detail View

**User Action:** Player clicks on Camera 12 button in the grid

### Step-by-Step Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. UI EVENT                                                 │
└─────────────────────────────────────────────────────────────┘

Player clicks button in grid
    ↓
CameraImageButton.OnButtonClick()
    ├─ Gets: cameraIndex = 12
    └─ Invokes callback: onClickCallback(12)
        ↓
        CameraGridUI.OnCameraImageClick(12)
            └─ Calls: ViewManager.SwitchToDetailView(12)

┌─────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER LAYER                                         │
└─────────────────────────────────────────────────────────────┘

ViewManager.SwitchToDetailView(12)
    │
    ├─ currentMode = ViewMode.DetailView
    │
    ├─ EnableOnlyCamera(12)
    │   ├─ Disables cameras[0..11].camera.enabled = false
    │   ├─ Enables  cameras[12].camera.enabled = true
    │   └─ Disables cameras[13..29].camera.enabled = false
    │
    ├─ SwitchToDetailTextures()
    │   └─ Assigns detailRenderTexture to camera[12].targetTexture
    │
    ├─ cameras[12].camera.Render()  // Force first frame
    │
    ├─ StopGridViewUpdates()  // Stop old coroutine
    │
    ├─ RestartDetailViewUpdates(12)
    │   └─ Starts: UpdateDetailTextureCoroutine(12)
    │       └─ Renders camera[12] every 0.1 seconds
    │
    ├─ Fires: OnModeChanged(ViewMode.DetailView)
    │
    └─ Fires: OnCameraSelected(12)

┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW LAYER                                               │
└─────────────────────────────────────────────────────────────┘

CameraGridUI receives OnModeChanged(DetailView)
    ↓
    CameraGridUI.HandleModeChanged(DetailView)
        └─ gridPanel.SetActive(false)  // Hide grid

DetailViewUI receives OnModeChanged(DetailView)
    ↓
    DetailViewUI.HandleModeChanged(DetailView)
        └─ detailPanel.SetActive(true)  // Show detail panel

DetailViewUI receives OnCameraSelected(12)
    ↓
    DetailViewUI.HandleCameraSelected(12)
        │
        ├─ currentCameraIndex = 12
        │
        ├─ UpdateDetailView()
        │   │
        │   ├─ detailCameraImage.texture = 
        │   │   ViewManager.GetDetailTexture(12)
        │   │
        │   ├─ bool hasStatic = 
        │   │   ViewManager.GetCameraData(12).hasStatic
        │   │
        │   ├─ detailCameraImage.gameObject.SetActive(!hasStatic)
        │   │
        │   ├─ detailStaticOverlay.gameObject.SetActive(hasStatic)
        │   │
        │   └─ cameraIndexText.text = "Cam13"  // 12+1 for display
        │
        └─ (View fully updated)

┌─────────────────────────────────────────────────────────────┐
│ 4. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ Camera 12 fills entire screen at high resolution
✅ Only Camera 12 rendering (29 other cameras disabled)
✅ Camera index displayed: "Cam13"
✅ Static overlay shown if camera is broken
✅ Grid hidden
✅ Previous/Next buttons active
```

---

## Scenario 3: Camera Navigation in Detail View

**User Action:** Player presses "D" key to view next camera

### Step-by-Step Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INPUT LAYER                                              │
└─────────────────────────────────────────────────────────────┘

InputHandler.Update() detects Input.GetKeyDown(KeyCode.D)
    ├─ Checks: ViewManager.GetCurrentMode() == DetailView
    └─ Calls: ViewManager.SwitchToNextCamera()

┌─────────────────────────────────────────────────────────────┐
│ 2. CONTROLLER LAYER                                         │
└─────────────────────────────────────────────────────────────┘

ViewManager.SwitchToNextCamera()
    │
    ├─ int current = detailViewUI.GetCurrentCameraIndex()
    │   └─ Returns: 12
    │
    ├─ int next = (12 + 1) % 30
    │   └─ Result: 13
    │
    └─ SwitchToDetailView(13)
        │
        ├─ EnableOnlyCamera(13)
        │   └─ Disables all except cameras[13]
        │
        ├─ RestartDetailViewUpdates(13)
        │
        ├─ Fires: OnModeChanged(DetailView)  // Mode unchanged
        │
        └─ Fires: OnCameraSelected(13)

┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW LAYER                                               │
└─────────────────────────────────────────────────────────────┘

DetailViewUI receives OnCameraSelected(13)
    ↓
    DetailViewUI.HandleCameraSelected(13)
        ├─ currentCameraIndex = 13
        ├─ detailCameraImage.texture = GetDetailTexture(13)
        ├─ Check cameras[13].hasStatic and update overlay
        └─ cameraIndexText.text = "Cam14"

┌─────────────────────────────────────────────────────────────┐
│ 4. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ Camera switched from 12 → 13
✅ Display updated: "Cam13" → "Cam14"
✅ Only one camera rendering (no performance spike)
✅ Seamless transition
```

**Note:** Wraparound logic ensures pressing "D" on Camera 30 returns to Camera 1.

---

## Scenario 4: Randomized Camera Failures

**Trigger:** Game initialization (ViewManager.Start)

### Step-by-Step Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INITIALIZATION                                           │
└─────────────────────────────────────────────────────────────┘

ViewManager.Start()
    ↓
    InitializeCameras()
        │
        ├─ Creates CameraData[30] array
        │   └─ Each with gridTexture, detailTexture, hasStatic=false
        │
        └─ RandomizeStaticCameras()

RandomizeStaticCameras()
    │
    ├─ staticCameraCount = 3  // Config value
    │
    ├─ Creates list: availableIndices = [0,1,2,...,29]
    │
    └─ For i = 0 to 2:
        ├─ randomIndex = Random.Range(0, availableIndices.Count)
        │   └─ Example: 7
        │
        ├─ cameraIndex = availableIndices[7]
        │   └─ Example: 18
        │
        ├─ Call: SetCameraStatic(18, true)
        │
        └─ Remove index from availableIndices

SetCameraStatic(18, true)  // Called 3 times for indices: 4, 18, 23
    │
    ├─ cameras[18].hasStatic = true  // Update model
    │
    └─ Fires: OnStaticChanged(18, true)

┌─────────────────────────────────────────────────────────────┐
│ 2. VIEW LAYER                                               │
└─────────────────────────────────────────────────────────────┘

CameraGridUI receives OnStaticChanged(18, true)
    ↓
    CameraGridUI.HandleStaticChanged(18, true)
        └─ cameraButtons[18].GetComponent<CameraStaticEffect>()
            .ShowStatic(true)
                ├─ staticOverlay.gameObject.SetActive(true)
                └─ cameraImage.gameObject.SetActive(false)

(This happens 3 times for indices: 4, 18, 23)

┌─────────────────────────────────────────────────────────────┐
│ 3. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ 3 random cameras show static effect
✅ Different cameras fail each playthrough
✅ State stored in CameraData, not UI
✅ UI updated via events, not polling

When player opens grid:
✅ Cameras 4, 18, 23 show static overlay
✅ Other 27 cameras show live feed
```

---

## Scenario 5: Floor Plan Interaction

**User Action:** Player interacts with blueprint object, switches floors, draws notes

### Step-by-Step Flow

```
┌─────────────────────────────────────────────────────────────┐
│ 1. OPENING FLOOR PLAN                                       │
└─────────────────────────────────────────────────────────────┘

Player clicks blueprint object
    ↓
InteractableObject.Interact()
    └─ ViewManager.SwitchToFloorPlanView()
        │
        ├─ currentMode = ViewMode.FloorPlan
        │
        ├─ firstPersonLook.enabled = false
        │
        ├─ Cursor.visible = true
        │
        └─ Fires: OnModeChanged(ViewMode.FloorPlan)

FloorPlanUI receives OnModeChanged(FloorPlan)
    ↓
    FloorPlanUI.HandleModeChanged(FloorPlan)
        │
        ├─ floorPlanPanel.SetActive(true)
        │
        └─ LoadFloorTexture(currentFloorIndex)
            │
            ├─ string key = "FloorPlan_Floor_0"
            ├─ Check PlayerPrefs.HasKey(key)
            │   └─ If exists: load saved annotations
            │   └─ If not: show clean blueprint
            │
            └─ drawSurface.SetTexture(loadedTexture)

┌─────────────────────────────────────────────────────────────┐
│ 2. DRAWING ANNOTATIONS                                      │
└─────────────────────────────────────────────────────────────┘

Player draws on blueprint
    ↓
UIDrawSurface.Update()
    │
    ├─ Detects Input.GetMouseButton(0)  // Left click
    │
    ├─ TryGetPixelPosition(out pixelPos)
    │   └─ Converts screen coordinates to texture coordinates
    │
    ├─ Draw(drawColor)
    │   └─ DrawLine(previousPixelPos, pixelPos, color)
    │       └─ For each step in line:
    │           DrawCircle(x, y, brushSize, color)
    │
    └─ drawTexture.Apply()  // GPU upload

┌─────────────────────────────────────────────────────────────┐
│ 3. SWITCHING FLOORS                                         │
└─────────────────────────────────────────────────────────────┘

Player clicks "Floor 2" button
    ↓
FloorPlanUI.SwitchFloor(1)
    │
    ├─ SaveCurrentFloor()
    │   ├─ currentFloorTextures[0] = drawSurface.GetTexture()
    │   └─ SaveFloorTexture(0, texture)
    │       ├─ byte[] bytes = texture.EncodeToPNG()
    │       ├─ string base64 = Convert.ToBase64String(bytes)
    │       └─ PlayerPrefs.SetString("FloorPlan_Floor_0", base64)
    │
    ├─ currentFloorIndex = 1
    │
    └─ drawSurface.SetTexture(currentFloorTextures[1])
        └─ Loads Floor 2 (with any previous annotations)

┌─────────────────────────────────────────────────────────────┐
│ 4. CLOSING FLOOR PLAN                                       │
└─────────────────────────────────────────────────────────────┘

Player presses ESC
    ↓
InputHandler.Update() detects KeyCode.Escape
    └─ ViewManager.SwitchToPanopticon()
        └─ Fires: OnModeChanged(Panopticon)

FloorPlanUI receives OnModeChanged(Panopticon)
    ↓
    FloorPlanUI.HandleModeChanged(Panopticon)
        │
        ├─ SaveCurrentFloor()  // Save before closing!
        │
        └─ floorPlanPanel.SetActive(false)

┌─────────────────────────────────────────────────────────────┐
│ 5. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ Annotations saved per floor per session
✅ Can switch between floors without losing notes
✅ Notes persist until new game started
✅ Saved as Base64 PNG in PlayerPrefs
```

---

## Scenario 6: Returning to Panopticon

**User Action:** Player presses ESC from any view mode

### State Transitions

```
From Camera Grid:
    ESC → Panopticon

From Detail View:
    ESC → Camera Grid
    ESC → Panopticon

From Floor Plan:
    ESC → Panopticon
```

### Step-by-Step Flow (from Detail View)

```
┌─────────────────────────────────────────────────────────────┐
│ 1. INPUT LAYER                                              │
└─────────────────────────────────────────────────────────────┘

InputHandler.Update() detects KeyCode.Escape
    ├─ Checks: currentMode == DetailView
    └─ Calls: ViewManager.SwitchToOverview()
        └─ (Returns to grid, not panopticon)

┌─────────────────────────────────────────────────────────────┐
│ 2. FROM GRID TO PANOPTICON                                  │
└─────────────────────────────────────────────────────────────┘

Player presses ESC again
    ↓
InputHandler.Update() detects KeyCode.Escape
    ├─ Checks: currentMode == CameraOverview
    └─ Calls: ViewManager.SwitchToPanopticon()

ViewManager.SwitchToPanopticon()
    │
    ├─ currentMode = ViewMode.Panopticon
    │
    ├─ StopGridViewUpdates()
    │   └─ Stops coroutine (no more camera rendering)
    │
    ├─ DisableAllCameras()
    │   └─ Sets camera.enabled = false for all 30 cameras
    │
    ├─ firstPersonLook.enabled = true
    │
    ├─ Cursor.visible = false
    ├─ Cursor.lockState = CursorLockMode.Locked
    │
    └─ Fires: OnModeChanged(ViewMode.Panopticon)

┌─────────────────────────────────────────────────────────────┐
│ 3. VIEW LAYER                                               │
└─────────────────────────────────────────────────────────────┘

All UI components receive OnModeChanged(Panopticon)
    │
    ├─ CameraGridUI.HandleModeChanged(Panopticon)
    │   └─ gridPanel.SetActive(false)
    │
    ├─ DetailViewUI.HandleModeChanged(Panopticon)
    │   └─ detailPanel.SetActive(false)
    │
    └─ FloorPlanUI.HandleModeChanged(Panopticon)
        └─ floorPlanPanel.SetActive(false)

┌─────────────────────────────────────────────────────────────┐
│ 4. RESULT                                                   │
└─────────────────────────────────────────────────────────────┘

✅ All UI hidden
✅ All cameras disabled (massive performance gain)
✅ First-person look enabled
✅ Cursor locked to center
✅ Player can move and interact with 3D objects again
```

---

## Performance Implications

### Camera Rendering Overhead

| Mode | Active Cameras | FPS Impact |
|------|---------------|------------|
| Panopticon | 0 | Baseline (60 FPS) |
| Camera Grid | 30 | -5 FPS (55 FPS) |
| Detail View | 1 | -1 FPS (59 FPS) |
| Floor Plan | 0 | Baseline (60 FPS) |

**Optimization:** Only render what's needed. Disabling 29 cameras in Detail View saves ~90% GPU time compared to Grid.

---

## Event Propagation Summary

```
User Action
    ↓
Input Layer (translate to command)
    ↓
ViewManager (update state, fire events)
    ↓
View Layer (subscribe, react)
    ↓
Visual Result
```

**Key Points:**
- ✅ Events propagate in **one direction** (no cycles)
- ✅ UI components **never talk to each other**
- ✅ ViewManager is **single source of truth**
- ✅ All state changes go through **events** (no polling)

---

See [Architecture.md](Architecture.md) for component details and [Performance.md](Performance.md) for optimization techniques.
