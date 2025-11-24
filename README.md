THE LAST OBSERVER — SHOWCASE

Experimental survival game about control, pressure and isolation.
This public repository contains a curated selection of scripts, systems and technical materials that illustrate how the core mechanics of the game are implemented.

Full Unity project remains private.
This repository is designed as a clean and readable technical showcase.

🎮 About the Game

The Last Observer places the player in a sealed panopticon room at the center of a multi-floor building. Surrounded by five doors, limited tools and a failing surveillance system, the player must survive as hostile NPCs move through the environment.

You cannot move.
You can only observe.

Your only advantage is your ability to control information.

🧩 Core Gameplay Features
📷 Camera System

Network of surveillance cameras with:

grid view

individual feed view

previous / next camera cycling

Optimized rendering for multiple camera feeds (no render texture spam)

Manual mapping mechanic: players annotate a building blueprint with camera numbers

🚪 Door System

Five player-controlled doors

Only one door can be closed at any moment

Enemies attempt entry from dynamically chosen routes

☎️ Phone Events

Simulated emergency hotline calls

Responses degrade over time, amplifying tension

👾 NPC Navigation

Implementation using Unity NavMesh

Randomized patrol routes

Event-driven behavior (lights out, broken cameras, new passages)

Future: procedural model variations through Blender plugin

💀 Game Loop

Survive a fixed amount of time

Win/lose states implemented

Expandable for future missions and modifiers

🛠️ Technical Highlights Included in This Repo

This showcase contains:

/Scripts
    CameraSystem/
    DoorSystem/
    Interaction/
    NPC/
    UI/
    Events/

/Docs
    technical_breakdown.md
    gameplay_flow.md
    system_diagrams.png

/Media
    screenshots/
    gifs/


What’s deliberately not included:

heavy assets

full Unity scenes

Blender files

build data

This keeps the repo light, readable, browser-friendly and recruiter-friendly.

🖥️ Tech Stack

Unity (URP)

C#

Blender (environment modeling & prototype NPC)

Custom tools for camera rendering & UI interaction

Planned procedural NPC mesh modifier (Blender Python API)

🌐 Project Demo

(Optional — update when available)

WebGL build

Video capture

Screenshot gallery

📚 Documentation

technical_breakdown.md — explanation of main systems

gameplay_flow.md — how the player interacts with the environment

future_plans.md — roadmap of upcoming features

🚀 Development Notes

This project is a solo development effort — all modeling, scripting, design and system architecture are created by me.

The public version is continuously evolving as I clean up and extract code from the full private repository.

📮 Contact

If you'd like to discuss collaboration or opportunities, feel free to reach out.
(Add email / website / portfolio link)
