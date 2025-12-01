# The Last Observer — Technical Overview

This document describes the technical architecture of the core systems used in The Last Observer.
The project is a first-person static survival game where the player interacts primarily through UI and environmental controls.

## Core Systems

### 1. Multi-Camera System
The game features:
- A **grid layout** showing all available camera feeds.
- A **single camera viewer** with next/previous cycling.
- Manual **camera-to-map linking**, where players annotate a blueprint.

Technical points:
- Secondary cameras render to RenderTextures.
- RenderTextures are optimized by lowering update frequency.
- UI dynamically displays or hides camera feeds.
- Switching feeds reassigns the active RenderTexture.

### 2. Door Control System
Five doors can be controlled by the player.
Rules:
- Only *one* door can be closed at any time.
- Closing a new door automatically opens the previous one.
- Doors react to NPC proximity.

### 3. NPC Navigation
NPCs follow:
- Random patrol routes
- Event-triggered behavior
- Navigation via Unity NavMesh

NPCs may:
- Disable lights
- Break cameras
- Discover new paths

### 4. Interaction System
The player interacts with:
- The computer (camera feeds)
- The building blueprint (annotations)
- The phone (story cues)
- The doors (survival system)

The interaction system uses raycasts and UI events.

### 5. Game Loop
- The goal is to survive a fixed amount of time.
- Win/loss states are triggered by room breaches or timer completion.

### 6. Rendering Optimization
Because many render textures are active:
- Camera updates are throttled
- Cameras are disabled outside of active view
- Low resolution textures used in grid mode

---
