# THE LAST OBSERVER 

**Experimental survival game about control, pressure and isolation**\
This public repository contains a curated selection of scripts, systems
and technical materials that illustrate how the core mechanics of the
game are implemented

> *Full Unity project remains private\
> This repository is designed as a clean and readable technical
> showcase*


<img src="Visuals/gifs/observer_demo.gif" width="100%">

## About the Game

**The Last Observer** places the player in a sealed panopticon room at
the center of a multi-floor building. Surrounded by five doors, limited
tools and a failing surveillance system, the player must survive as
hostile NPCs move through the environment.

You cannot move.\
You can only observe.

Your only advantage is your ability to control information.

## Core Gameplay Features

### Camera System

-   Network of surveillance cameras with:
    -   **grid view**
    -   **individual feed view**
    -   **previous / next** camera cycling\
-   Optimized rendering for multiple camera feeds
-   Manual mapping mechanic: players annotate a building blueprint with
    camera numbers

### Door System

-   Five player-controlled doors
-   Only **one door can be closed at a time**
-   Enemies attempt entry from dynamically chosen routes

### Phone Events

-   Simulated emergency hotline calls
-   Responses degrade over time

### NPC Navigation

-   Unity NavMesh
-   Randomized patrol routes
-   Event-driven behavior (lights out, broken cameras, new passages)

### Game Loop

-   Survive a fixed amount of time
-   Win/lose states implemented

## Technical Highlights Included in This Repo

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

## Tech Stack

-   Unity (URP)
-   C#
-   Blender
-   Custom tools for camera rendering & UI interaction

## Project Demo

*(Optional --- update when available)*

## Documentation

-   **technical_breakdown.md**
-   **gameplay_flow.md**
-   **future_plans.md**

## Development Notes

This project is a solo development effort --- all modeling, scripting,
design and system architecture are created by me.

## Contact

Add email / website / portfolio link here.

