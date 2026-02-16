# System Architecture

**Surveillance Camera System - Technical Breakdown**

---

## Table of Contents

1. [Overview](#overview)
2. [Architecture Diagram](#architecture-diagram)
3. [Design Patterns](#design-patterns)
4. [Layer Breakdown](#layer-breakdown)
5. [Component Details](#component-details)
6. [Before & After Refactor](#before--after-refactor)

---

## Overview

The surveillance system is built on a **three-layer architecture** with strict separation of concerns:

```
Input Layer (commands) → Controller Layer (mediator) → View Layer (UI)
                              ↓
                         Model Layer (data)
```

### Core Principles

✅ **Single Source of Truth** - `CameraData[]` array holds all camera state  
✅ **Event-Driven** - UI components never poll, only subscribe  
✅ **Zero Coupling** - View components don't know about each other  
✅ **Testable** - Each layer can be unit tested independently  

---

## Architecture Diagram

```
┌─────────────────────────────────────────────────────────────────┐
│                         INPUT LAYER                             │
│                                                                 │
│  ┌──────────────────────┐      ┌─────────────────────────┐     │
│  │   InputHandler       │      │  PlayerInteraction      │     │
│  │   (Keyboard/Mouse)   │      │  (3D Raycast)           │     │
│  └──────────┬───────────┘      └───────────┬─────────────┘     │
│             │                               │                   │
└─────────────┼───────────────────────────────┼───────────────────┘
              │                               │
              │  Commands                     │  Commands
              ▼                               ▼
┌─────────────────────────────────────────────────────────────────┐
│                      CONTROLLER LAYER                           │
│                                                                 │
│                     ┌──────────────────┐                        │
│                     │   ViewManager    │ ◄── Mediator Pattern   │
│                     │   (Singleton)    │                        │
│                     └────────┬─────────┘                        │
│                              │                                  │
│        ┌─────────────────────┼─────────────────────┐           │
│        │                     │                     │           │
│        │                     │                     │           │
│   Owns & Manages        Fires Events          Controls         │
│        │                     │                     │           │
│        ▼                     ▼                     ▼           │
│  ┌──────────┐        ┌──────────────┐      ┌──────────────┐   │
│  │ Camera   │        │ OnModeChanged│      │   Camera     │   │
│  │ Data[]   │        │ OnStatic...  │      │   Enable/    │   │
│  │ (Model)  │        │ OnCamera...  │      │   Coroutines │   │
│  └──────────┘        └──────────────┘      └──────────────┘   │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
              │                     │                     │
              │                     │                     │
              ▼                     ▼                     ▼
┌─────────────────────────────────────────────────────────────────┐
│                         VIEW LAYER (UI)                         │
│                                                                 │
│  ┌───────────────┐  ┌───────────────┐  ┌──────────────────┐   │
│  │ CameraGridUI  │  │ DetailViewUI  │  │  FloorPlanUI     │   │
│  │               │  │               │  │  + DrawSurface   │   │
│  └───────┬───────┘  └───────┬───────┘  └────────┬─────────┘   │
│          │                  │                    │             │
│          └──────────────────┴────────────────────┘             │
│                             │                                  │
│              All subscribe to ViewManager events               │
│              No direct communication between views             │
│                                                                 │
└─────────────────────────────────────────────────────────────────┘
```

---

## Design Patterns

### 1. Mediator Pattern (ViewManager)

**Problem:**  
Without a mediator, UI components would need to know about each other, creating a tightly coupled mesh of dependencies.

**Solution:**  
`ViewManager` acts as central hub - all communication goes through it.

```
❌ Before:
CameraGridUI ←→ DetailViewUI ←→ FloorPlanUI
    ↕              ↕              ↕
  Camera1 ←─→  Camera2 ←─→  Camera3
  
✅ After:
CameraGridUI ──┐
               │
DetailViewUI ──┼──► ViewManager ──► CameraData[]
               │
FloorPlanUI ───┘
```

---

### 2. Observer Pattern (Events)

**Problem:**  
How do UI components know when camera state changes without polling every frame?

**Solution:**  
Event-driven updates via C# `Action` delegates.

```csharp
// ViewManager fires events
public event Action<ViewMode> OnModeChanged;
public event Action<int, bool> OnStaticChanged;
public event Action<int> OnCameraSelected;

// UI components subscribe
void OnEnable() {
    ViewManager.Instance.OnModeChanged += HandleModeChanged;
}
```

**Benefits:**
- Zero CPU overhead (no Update() loops checking state)
- Instant reactions to state changes
- UI components can be enabled/disabled without breaking system

---

### 3. State Machine (View Modes)

**States:**
```
Panopticon → CameraGrid → DetailView → FloorPlan
     ↑          ↓             ↓            ↓
     └──────────┴─────────────┴────────────┘
              (ESC key returns to Panopticon)
```

**Implementation:**
```csharp
public enum ViewMode {
    Panopticon,     // First-person, free look
    CameraOverview, // Grid of all cameras
    DetailView,     // Single camera full-screen
    FloorPlan       // Blueprint with annotations
}
```

Each mode has:
- Entry actions (enable cameras, show UI)
- Exit actions (disable cameras, hide UI)
- Valid transitions (can't skip from Panopticon to DetailView)

---

### 4. Singleton Pattern (ViewManager)

**Justification:**
- Only one ViewManager should exist per scene
- Needs global access from input handlers, UI, and gameplay systems
- Prevents accidental instantiation of multiple managers

**Implementation:**
```csharp
public static ViewManager Instance { get; private set; }

void Awake() {
    if (Instance == null) {
        Instance = this;
    } else {
        Destroy(gameObject);
    }
}
```

---

## Layer Breakdown

### Input Layer

| Component | Type | Responsibility |
|-----------|------|----------------|
| `InputHandler` | MonoBehaviour | Global keyboard commands (ESC, Tab, A/D) |
| `PlayerInteraction` | MonoBehaviour | 3D raycast for monitor/blueprint objects |

**Key Point:** Input layer **does not contain game logic** - only translates user actions into commands for ViewManager.

---

### Controller Layer (Mediator)

| Component | Type | Responsibility |
|-----------|------|----------------|
| `ViewManager` | Singleton MonoBehaviour | FSM, camera control, event firing |

**Owns:**
- `CameraData[] cameras` - array of all camera state
- Current `ViewMode` enum value
- Coroutines for rendering

**Does NOT own:**
- UI GameObjects (owned by respective UI scripts)
- Input state (owned by InputHandler)

---

### Model Layer

| Component | Type | Responsibility |
|-----------|------|----------------|
| `CameraData` | Plain C# class | Holds camera state (textures, static flag, position) |

**Structure:**
```csharp
public class CameraData {
    public Camera camera;               // Unity Camera component
    public RenderTexture gridTexture;   // 170×128 low-res
    public RenderTexture detailTexture; // 680×512 high-res
    public bool hasStatic;              // Is camera broken?
    public int index;                   // Camera identifier (0-29)
    public Vector3 worldPosition;       // For floor plan markers
}
```

**Why not MonoBehaviour?**  
Data models should be plain classes for testability. ViewManager instantiates them during initialization.

---

### View Layer (UI)

| Component | Role | Subscribes To |
|-----------|------|---------------|
| `CameraGridUI` | Grid of 30 cameras | `OnModeChanged`, `OnStaticChanged` |
| `CameraImageButton` | Single button in grid | N/A (instantiated by Grid) |
| `DetailViewUI` | Full-screen camera view | `OnCameraSelected`, `OnStaticChanged` |
| `FloorPlanUI` | Blueprint + notes | `OnModeChanged` |
| `UIDrawSurface` | Drawing system | N/A (used by FloorPlan) |
| `CameraStaticEffect` | Visual overlay | N/A (controlled by GridUI) |

**Critical Rule:** View components **never call other view components**. All data flows from ViewManager.

---

## Component Details

### ViewManager.cs

**Public API:**

```csharp
// Mode Switching
public void SwitchToOverview()
public void SwitchToDetailView(int cameraIndex)
public void SwitchToFloorPlanView()
public void SwitchToPanopticon()
public void SwitchToNextCamera()
public void SwitchToPreviousCamera()

// Camera State
public void SetCameraStatic(int index, bool isStatic)
public CameraData GetCameraData(int index)
public RenderTexture GetGridTexture(int index)
public RenderTexture GetDetailTexture(int index)
public int GetCameraCount()

// Query State
public ViewMode GetCurrentMode()

// Events
public event Action<ViewMode> OnModeChanged;
public event Action<int, bool> OnStaticChanged;
public event Action<int> OnCameraSelected;
```

**Rendering Strategy:**

```csharp
// Grid Mode - all cameras enabled
foreach (Camera cam in cameras) {
    cam.enabled = true;
}
StartCoroutine(UpdateGridTexturesCoroutine());

// Detail Mode - only one camera enabled
EnableOnlyCamera(selectedIndex);
StartCoroutine(UpdateDetailTextureCoroutine(selectedIndex));

// Panopticon/FloorPlan - all cameras disabled
DisableAllCameras();
```

---

### CameraData.cs

**Lifecycle:**

1. **Creation** (ViewManager.InitializeCameras):
```csharp
for (int i = 0; i < securityCameras.Length; i++) {
    cameras[i] = new CameraData(
        securityCameras[i],
        i,
        gridImageSize,
        detailImageSize
    );
}
```

2. **Usage** (throughout gameplay):
```csharp
// Get texture for UI
RenderTexture tex = cameras[5].gridTexture;

// Check if camera broken
bool broken = cameras[5].hasStatic;
```

3. **Cleanup** (ViewManager.OnDestroy):
```csharp
foreach (CameraData data in cameras) {
    data.Release();
}
```

---

### CameraGridUI.cs

**Event Flow:**

```csharp
void OnEnable() {
    // Subscribe to ViewManager
    ViewManager.Instance.OnModeChanged += HandleModeChanged;
    ViewManager.Instance.OnStaticChanged += HandleStaticChanged;
}

void HandleModeChanged(ViewMode mode) {
    if (mode == ViewMode.CameraOverview) {
        ShowGrid();
        RefreshAllTextures();
    } else {
        HideGrid();
    }
}

void HandleStaticChanged(int cameraIndex, bool hasStatic) {
    cameraButtons[cameraIndex].ShowStatic(hasStatic);
}
```

**Button Generation:**

```csharp
void InitializeCameraGrid() {
    int cameraCount = ViewManager.Instance.GetCameraCount();
    
    for (int i = 0; i < cameraCount; i++) {
        GameObject buttonObj = Instantiate(cameraImagePrefab, gridContainer);
        CameraImageButton button = buttonObj.GetComponent<CameraImageButton>();
        
        button.Initialize(
            i,
            ViewManager.Instance.GetGridTexture(i),
            OnCameraImageClick // callback
        );
        
        cameraButtons.Add(button);
    }
}
```

---

### DetailViewUI.cs

**Navigation Logic:**

```csharp
void HandleKeyboardInput() {
    if (Input.GetKeyDown(KeyCode.A)) {
        ViewManager.Instance.SwitchToPreviousCamera();
    }
    if (Input.GetKeyDown(KeyCode.D)) {
        ViewManager.Instance.SwitchToNextCamera();
    }
}
```

**In refactored version**, this moves to `InputHandler` and ViewManager handles wraparound:

```csharp
// ViewManager
public void SwitchToNextCamera() {
    int current = detailViewUI.GetCurrentCameraIndex();
    int next = (current + 1) % cameras.Length;
    SwitchToDetailView(next);
}
```

---

### FloorPlanUI.cs

**Persistence Strategy:**

```csharp
// Save current floor annotations
void SaveCurrentFloor() {
    Texture2D texture = drawSurface.GetTexture();
    byte[] bytes = texture.EncodeToPNG();
    string base64 = Convert.ToBase64String(bytes);
    
    PlayerPrefs.SetString($"FloorPlan_Floor_{currentFloorIndex}", base64);
}

// Load on floor switch
Texture2D LoadFloorTexture(int floorIndex) {
    string key = $"FloorPlan_Floor_{floorIndex}";
    if (!PlayerPrefs.HasKey(key)) return null;
    
    string base64 = PlayerPrefs.GetString(key);
    byte[] bytes = Convert.FromBase64String(base64);
    
    Texture2D texture = new Texture2D(2, 2);
    texture.LoadImage(bytes);
    return texture;
}
```

**Why PlayerPrefs?**  
Session persistence only (notes cleared on new game). For permanent save system, would use JSON serialization instead.

---

## Before & After Refactor

### Problem: Tight Coupling

**Original Code (DetailViewUI):**
```csharp
private bool CheckIfCameraHasStaticEffect(int cameraIndex) {
    // ❌ Accessing another UI component's internal state
    var cameraButtons = ViewManager.Instance.cameraGridUI.GetCameraButtons();
    var staticEffect = cameraButtons[cameraIndex].GetComponent<CameraStaticEffect>();
    return staticEffect.isStatic;
}
```

**Issues:**
1. DetailViewUI knows about CameraGridUI (coupling)
2. DetailViewUI knows about CameraStaticEffect structure (fragile)
3. State (`isStatic`) stored in view instead of model
4. Can't test DetailViewUI without full CameraGridUI setup

---

### Solution: Event-Driven Model State

**Refactored Code:**

**ViewManager:**
```csharp
public void SetCameraStatic(int index, bool isStatic) {
    cameras[index].hasStatic = isStatic; // Update model
    OnStaticChanged?.Invoke(index, isStatic); // Fire event
}
```

**DetailViewUI:**
```csharp
void OnEnable() {
    ViewManager.Instance.OnStaticChanged += HandleStaticChanged;
}

void HandleStaticChanged(int cameraIndex, bool hasStatic) {
    if (cameraIndex == currentCameraIndex) {
        detailStaticOverlay.gameObject.SetActive(hasStatic);
    }
}
```

**CameraGridUI:**
```csharp
void HandleStaticChanged(int cameraIndex, bool hasStatic) {
    cameraButtons[cameraIndex].ShowStatic(hasStatic);
}
```

**Benefits:**
- ✅ DetailViewUI and CameraGridUI don't know about each other
- ✅ State lives in `CameraData` (single source of truth)
- ✅ Both UI components react independently to same event
- ✅ Can test each component in isolation

---

### Visual Comparison

**Before (Mesh of Dependencies):**
```
DetailViewUI ──┬──► ViewManager
               │
               └──► CameraGridUI ──► CameraImageButton ──► CameraStaticEffect
                                                                    ↓
                                                              isStatic field
```

**After (Star Topology):**
```
                           ViewManager
                          ┌─────┴─────┐
                          │ CameraData│
                          │  hasStatic│
                          └─────┬─────┘
                                │
                    Fires OnStaticChanged
                                │
                ┌───────────────┼───────────────┐
                ▼                               ▼
          DetailViewUI                    CameraGridUI
       (subscribes, updates)          (subscribes, updates)
```

---

## Summary

The refactored architecture achieves:

✅ **Separation of Concerns** - Input, Controller, Model, View all isolated  
✅ **Single Responsibility** - Each component has one clear job  
✅ **Dependency Inversion** - UI depends on events, not concrete implementations  
✅ **Open/Closed** - Can add new view modes without modifying existing code  
✅ **Testability** - Each layer mockable and unit-testable  

This structure scales well for additional features like:
- Recording/playback system
- AI detection overlays
- Multi-player spectator mode
- VR support

See [DataFlow.md](DataFlow.md) for detailed event propagation examples.
