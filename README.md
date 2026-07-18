# THE LAST OBSERVER 
*Technical Showcase — Unity Gameplay Systems & Tools*

**Experimental survival game about control, pressure and isolation**

This repository contains a technical showcase of core gameplay systems developed for The Last Observer
It demonstrates the architecture, gameplay mechanics, and custom tools behind a surveillance-based survival experience, including camera management, NPC navigation, interactive UI systems, and asset production workflows
The complete Unity project remains private. This repository focuses on selected systems and documentation that illustrate the engineering approach behind the game.

![Gameplay Demo](Media/gifs/observer_demo.gif)

---

## About the Game

**The Last Observer** places the player in a sealed panopticon room at the center of a multi-floor building.
Surrounded by five doors and a failing surveillance system, the player must survive as hostile NPCs move through the environment

**You cannot move. You can only observe**

Your objective is to survive a fixed-duration session while hostile NPCs navigate through the building
You cannot move or fight - your only tools are observation, information management, and limited environmental control

---

## Design Goals

The game explores decision-making under uncertainty.

By removing direct movement and combat, the player is forced to rely on observation, prediction, and resource management.

Every action is a trade-off:
- closing one door leaves others vulnerable
- focusing on one camera reduces awareness of the rest of the building
- incomplete information creates pressure and uncertainty

---

## Core Systems

##  Gameplay Loop
- Survive a fixed-duration session
- Observe NPC activity through surveillance cameras
- Analyze incomplete information using the building map
- Make limited defensive decisions by controlling doors
- Adapt to dynamic events that change the environment
  
### Surveillance System
- Network of 30+ security cameras across multiple floors
- **Grid View** - monitor all cameras simultaneously
- **Detail View** - examine individual feeds with navigation
- **Floor Plan** - annotate blueprints to track camera locations and passages condition
- Randomized broken cameras for incomplete information each playthrough

### NPC Behavior
- Unity NavMesh-based pathfinding
- Event-driven behavior (lights out, broken cameras, opened passages)

### Door Control
- Five player-controlled doors surrounding the panopticon
- **Only one door can be closed at a time**
- NPCs dynamically choose entry routes

---

## Technical Highlights

- Decoupled game architecture using mediator-based communication
- Event-driven UI systems with minimal dependencies between components
- Custom surveillance rendering pipeline using RenderTextures
- Persistent interactive map annotation system
- Modular NPC and event systems designed for future expansion

**→ [Full Architecture Documentation](Documentation/Architecture.md)**

---

## Tools & Pipeline

### Asset Production Pipeline
Custom Blender → Unity workflow for automated texture baking and material setup:
- Blender addon bakes procedural materials → Albedo, Normal, MRAO maps
- Unity import script auto-configures texture settings and compression
- Naming convention-based material setup

**→ [Blender Addon Repository](https://github.com/khazanovanastasia/blender-addons/tree/main/blender_unity_baker)**

---

## Repository Structure

```
/Scripts
    /Core               # ViewManager, CameraData, InputHandler
    /Camera             # Camera controllers and rendering
    /UI                 # Grid, Detail, FloorPlan UI components
    /Interaction        # 3D object interaction system
    /NPC                # Enemy AI and navigation
    /DoorSystem         # Door control mechanics
    /Events             # Phone and game event systems
    /Editor             # Material import pipeline tool

/Documentation
    Architecture.md     # System design and data flow
    Refactoring.md      # Before/after architecture improvements
    Performance.md      # Optimization strategies

/Media
    /gifs              # Gameplay demonstrations
    /screenshots       # UI mockups and system visualizations
```

---

## Tech Stack

- **Unity 2022.3.62f3** (Built-in)
- **C#**
- **Blender** (3D modeling, procedural materials)
- **Python** (Blender API scripting, pipeline automation)
- Custom Unity Editor tools
- Custom camera rendering & UI interaction tools

---

## Documentation

- **[Architecture Overview](Documentation/Architecture.md)** — System design, patterns, and component interactions
- **[Refactoring Journey](Documentation/Refactoring.md)** — Before/after architecture improvements
- **[Performance Notes](Documentation/Performance.md)** — Optimization techniques and benchmarks

---

## Development Status

The project is currently in active development

Core gameplay systems, AI, surveillance mechanics, and technical architecture are implemented
Current development focus is content production: environment creation, asset production, and level design

---

## Contact

https://khazanovanastasia.ru

khasanovanastasia@gmail.com
