# Systems Breakdown — The Last Observer

## 1. Camera Subsystem
- RenderTexture-based
- Individual camera classes manage feed state
- Cameras generate events when they go offline
- Cameras can be broken by NPCs

## 2. Door Subsystem
- DoorController manages 5 door states
- Only one door can be closed
- NPCs interact by detecting door states

## 3. NPC AI Subsystem
- NavMeshAgent movement
- Randomized patrols
- Event-based triggers:
  - blackout
  - new passages opening
  - camera sabotage

## 4. Interaction Subsystem
Handles:
- UI buttons
- Raycasts
- Context-dependent hover states
- Highlighting interactable objects

---
