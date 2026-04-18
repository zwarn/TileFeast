# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

TileFeast is a tile placement puzzle game built in Unity 6000.0.23f1. Players place puzzle pieces on a grid board; each placed piece is evaluated against "happiness rules" that assign it an emotion (Happy/Neutral/Sad), and scenarios are won by satisfying "completion rules" over the resulting emotion state.

## Build & Development

Unity project — open with Unity Hub (version 6000.0.23f1). `TileFeast.sln` for IDE integration (Rider/Visual Studio). There is no CLI test suite; EditMode tests live in `TileFeast.Tests.EditMode.csproj` and run via the Unity Test Runner.

Key packages: URP 17.0.3, Input System 1.11.2, Zenject (Plugins), Sirenix Odin Inspector (Plugins).

## Architecture

### Dependency Injection

All controllers are bound in `Assets/Scripts/Infrastructure/MainInstaller.cs` as `FromInstance` singletons (serialized scene references). When adding a new controller, register it there.

Currently bound: `ToolController`, `BoardController`, `PieceSupplyController`, `RulesController`, `ScenarioController`, `ScenarioPersistence`, `EditorModeController`, `GameController`, `HighlightController`, `ZoneController`, `CameraController`, `PieceRepository`, `SolverRunner`, plus individual tool instances (`GrabTool`, `ZoneTool`, `ShapeTool`).

### Core flow

- `Core/GameState.cs` — immutable-ish container: `GridSize`, blocked positions, `PlacedPieces`, `AvailablePieces`, `PieceInHand`, `HappinessRules`, `CompletionRules`, `Zones`.
- `Core/GameController.cs` — central coordinator. `LoadScenario(ScenarioSO)`, `SpawnPiece`, `ReturnAllNonLockedPiecesToSupply`. Fires `OnChangeGameState` and `OnBoardChanged`.
- `Board/BoardController.cs` — grid management. `PlacePiece`, `RemovePiece`, `IsValid`, `GetPieceByPosition`. Events: `OnBoardReset`, `OnPiecePlaced`, `OnPieceRemoved`.
- `Tools/ToolController.cs` — routes input to the active `ToolBase`. Tools include `GrabTool` (pick/place), `ZoneTool`, `ShapeTool`, wall/draw tools, etc.

### Piece / shape model

- `Piece` — base data (shape, sprite, aspects, locked flag). `PieceSO` is the asset form. `PieceWithRotation` = piece + rotation. `PlacedPiece` = piece + rotation + `Position`.
- `Pieces/ShapeHelper.cs` — shape math: `Rotate`, `Normalize`, `AreShapesEqual`, `GetAllNormalizedRotations` (used for deduplication by the solver).
- Rotation convention: 0 = identity, 1 = 90° CW (−y,x), 2 = 180° (−x,−y), 3 = 270° CW (y,−x). `PlacedPiece.GetTilePosition()` applies rotation then translates by `Position`.

### Emotion rule system (`Assets/Scripts/Rules/`)

Replaces the older PlacementRule/ScoreRule/ZoneRule model — do NOT add new rules in those old shapes.

- `HappinessRuleSO` — stateless `Evaluate(PlacedPiece, EmotionContext) → EmotionEffect?`. Implementations live under `Rules/EmotionRules/`.
- `EmotionEffect` — `{ PieceEmotion Emotion, string Reason, HappinessRuleSO Source }`.
- `PieceEmotionState` — a piece's accumulated effects; `FinalEmotion` = most negative wins (`PieceEmotion` enum: Happy=0, Neutral=1, Sad=2; cast to int).
- `EmotionEvaluationResult` — aggregated `PieceStates`, `HappyCount`, `NeutralCount`, `SadCount`. Score = `HappyCount`.
- `CompletionRuleSO` — stateless `IsMet(EmotionEvaluationResult, GameState)`. Implementations under `Rules/CompletionRules/`.
- `RulesController` — evaluates state and fires `OnEvaluationChanged`, `OnHappinessRulesReset`, `OnCompletionRulesReset`.
- `Rules/RulesHelper.cs` — `ConvertTiles(Dict<Vector2Int, PlacedPiece>, w, h)`, `GetGroups`, `GetNeighborPieces` — use these rather than reimplementing adjacency.
- `Rules/` also contains `AspectSources/`, `Checks/`, `Filters/`, `Components/`, `Conclusions/` — composable building blocks used to assemble rule SOs in the inspector.

Zones are pure spatial markers (`Zones/Zone.cs`, no embedded rule); any zone-aware behavior comes from a happiness rule that consults `EmotionContext.Zones`.

### Scenario

`Scenarios/ScenarioSO.cs` — level asset. Holds grid size, blocked positions, available + locked pieces, happiness rules, completion rules, zones, next-level reference. `ScenarioController` loads; `ScenarioPersistence` serializes edits from the in-game editor.

### Solver (`Assets/Scripts/Solver/`)

Exhaustive backtracking auto-solver. Construction must happen on the Unity main thread (it clones ScriptableObjects via `Object.Instantiate`); `SolveAsync(CancellationToken)` runs on a background `Task.Run`. Precomputes unique rotations per piece using `ShapeHelper` dedup. Records solutions where all `CompletionRule`s pass; score = `HappyCount`. Thread-safe via volatile counters + lock on `_results`. `SolverRunner` is the MonoBehaviour orchestrator; `UI.Solver.SolverPanel` shows results.

### UI pattern

Panels instantiate entries via `_container.InstantiatePrefab(prefab, parent)` so DI resolves on children. Entry scripts are MonoBehaviours with `SetRule()` / `SetData()` methods, wiring events in `OnEnable`/`OnDisable`. TMP_Text for labels. `UI/` mirrors the domain: `UI/Rules/`, `UI/Scenarios/`, `UI/Solver/`, `UI/Tools/`, `UI/Zones/`, `UI/Pieces/`, `UI/Common/`.

## Directory map

```
Assets/Scripts/
├── Board/           BoardController, view
├── Cameras/         CameraController
├── Core/            GameController, GameState
├── Infrastructure/  MainInstaller (Zenject bindings)
├── Pieces/          Piece data, aspects, supply, ShapeHelper
├── Rules/           Emotion/happiness/completion rules + building blocks
├── Scenarios/       ScenarioSO, ScenarioController, persistence
├── Solver/          AutoSolver, SolverRunner
├── Tools/           GrabTool, ZoneTool, ShapeTool, wall/draw/resize/etc.
├── UI/              Panels mirroring domain folders
└── Zones/           Zone spatial markers, ZoneController

Assets/ScriptableObjects/   # Pieces, Scenario, Rules, Zones, Aspects, Tool
Assets/Editor/              # PieceBatchCreatorWindow, SpriteSheetCopier
```

Namespaces follow folder names (`Pieces`, `Core`, `Board`, `Rules`, `Rules.CompletionRules`, `Zones`, `Scenarios`, `Solver`, `UI.Solver`, `UI.Rules`, `Infrastructure`).

## Input

- **Q / E / Mouse Scroll** — rotate piece in hand
- **Left Click** — primary tool action (place / pick up via `GrabTool`)
- **Right Click** — tool secondary action

## Creating new content

- **Piece:** `PieceSO` under `Assets/ScriptableObjects/Pieces/` (or use Editor → Piece Batch Creator).
- **Scenario:** `ScenarioSO` under `Assets/ScriptableObjects/Scenario/` — configure grid, pieces, happiness + completion rules, zones.
- **Happiness rule:** subclass `HappinessRuleSO` under `Rules/EmotionRules/`. Prefer composing existing `AspectSources` / `Checks` / `Filters` / `Conclusions` components over bespoke logic.
- **Completion rule:** subclass `CompletionRuleSO` under `Rules/CompletionRules/`.
- **Tool:** subclass `ToolBase` under `Tools/`, bind the instance in `MainInstaller`.
