# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TileFeast is a tile placement puzzle game built in Unity 6000.0.23f1. Players place puzzle pieces on a grid board, with scoring based on connected aspects (piece attributes) and zone rules.

## Build & Development

This is a Unity project - open with Unity Hub or Unity Editor (version 6000.0.23f1).

**Solution:** `TileFeast.sln` for IDE integration (Rider, Visual Studio)

**Key packages:**
- Universal Render Pipeline (URP) 17.0.3
- Input System 1.11.2
- Zenject (dependency injection) - via Plugins folder
- Sirenix Odin Inspector - via Plugins folder

## Architecture

### Dependency Injection

Uses Zenject framework. All main controllers are registered in `Assets/Scripts/Zenject/MainInstaller.cs`:
- `GameController` - Central game logic and input handling
- `BoardController` - Piece placement/removal on grid
- `ToolController` - Player tool/hand management
- `PieceSupplyController` - Available pieces inventory
- `RulesController` - Rule validation
- `ScenarioController` - Level loading
- `HighlightController` - Visual placement feedback

### Core Systems

**GameState** (`Core/GameState.cs`) - Holds all game state:
- Grid size, blocked positions
- Placed pieces, available pieces, piece in hand
- Active placement rules, score rules, zones

**GameController** (`Core/GameController.cs`) - Central coordinator:
- Loads scenarios via `LoadScenario(ScenarioSO)`
- Routes input to ToolController (Q/E or mouse scroll for rotation, right-click)
- Fires `OnChangeGameState` and `OnBoardChanged` events

**BoardController** (`Board/BoardController.cs`) - Grid management:
- Validates placement (bounds, blocked, empty)
- Tracks pieces by position via dictionary
- Events: `OnBoardReset`, `OnPiecePlaced`, `OnPieceRemoved`

### Data Model

**Piece types:**
- `Piece` - Base piece data (shape, sprite, aspects, locked flag)
- `PieceSO` - ScriptableObject asset defining a piece
- `PieceWithRotation` - Piece instance with rotation state
- `PlacedPiece` - Piece placed on board with position

**ScenarioSO** (`Scenario/ScenarioSO.cs`) - Level configuration asset containing:
- Grid size and blocked positions
- Available and locked pieces
- Placement rules, score rules, zones
- Next level reference

### Rules System

Located in `Assets/Scripts/Rules/`:

**Placement Rules** (`PlacementRules/`) - Validate piece placement:
- `PlacementRuleSO` - Base class
- `PlacementRuleAspectAdjacencySO` - Aspect adjacency requirements

**Score Rules** (`ScoreRules/`) - Calculate points:
- `ScoreRuleSO` - Base class
- `BiggestConnectedAspectScoreRuleSO` - Points for connected aspects

**Zone Rules** (`Board/Zone/`) - Region-specific rules:
- `OnlyAspectAllowedRuleSO` - Restrict aspects in zones
- `LeaveEmptyForBonusRuleSO` - Bonus for empty spaces

### Tool System

Located in `Assets/Scripts/Hand/Tool/`:
- `ITool` interface for tool actions
- `GrabTool` - Main tool for picking up and placing pieces
- `ToolController` - Manages active tool, routes input

## Key Directories

```
Assets/Scripts/
├── Board/           # Board controller, view, zones
├── Core/            # GameController, GameState
├── Hand/Tool/       # Player tool system
├── Piece/           # Piece data, aspects, supply
├── Rules/           # Placement and score rules
├── Scenario/        # Level configuration
├── UI/              # UI components
└── Zenject/         # DI installer

Assets/ScriptableObjects/  # Game data assets (pieces, scenarios, rules)
Assets/Editor/             # Editor tools (PieceBatchCreatorWindow, SpriteSheetCopier)
```

## Input

- **Q/E or Mouse Scroll** - Rotate piece in hand
- **Left Click** - Place/pick up piece (via ToolController)
- **Right Click** - Tool secondary action

## Creating New Content

**New Piece:** Create PieceSO asset in `Assets/ScriptableObjects/Pieces/` or use Editor > Piece Batch Creator

**New Scenario:** Create ScenarioSO asset in `Assets/ScriptableObjects/Scenario/` - configure grid size, pieces, rules, zones

**New Rule:** Extend `PlacementRuleSO` or `ScoreRuleSO` in the appropriate Rules subfolder
