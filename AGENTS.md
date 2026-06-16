# AGENTS.md — Color Rings Unity Project

## Project Overview

Unity 6 (6000.3.10f1) puzzle game — rotate and slide colored ring segments to match colors. URP 2D rendering. Target: Android/iOS.

## Tech Stack

- **Zenject** for DI (project-scoped via `ProjectContextInstaller` → `ScriptableObjectInstaller`)
- **UniTask** for async/await (replaces coroutines in services)
- **DOTween** for animations
- **Unity Addressables** for asset loading
- **Unity Input System** (new) for input
- **TextMeshPro** for UI text

## Architecture

Feature-based modular layout. Each module under `Assets/Feature/<ModuleName>/` contains:
- Service interfaces (`I*Service`) and implementations
- Models (plain C# data holders, not MonoBehaviours)
- MonoBehaviours for view/visual components
- ScriptableObject configs
- A Zenject installer (`Installers/*ModuleInstaller.cs`)
- Presenters (MVP pattern for UI views)

**25 module installers** are registered in `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs`.

### Scenes (build order)
1. `Assets/Scenes/InitScene.unity` — Bootstrap (loads first, runs `GameStateMachine`)
2. `Assets/Scenes/MainMenu.unity` — Main menu
3. `Assets/Scenes/GameSimple.unity` — Gameplay

### State Flow
`BootstrapState` → `MainMenuState` → `GameSimpleState`

## Editor Tools

| Menu Path | Purpose |
|-----------|---------|
| `Tools/GenerateAdresablesNames` | Regenerates `Assets/Scripts/AddressConstants.cs` from Addressable entries |
| `Tools/ColorRings/Level Generator` | Level designer with A* solver and difficulty rating |
| `Tools/UI/Create View Module` | Scaffolds a new MVP View module (View + Presenter + installer registration) |
| `Tools/Save Data/Clear All Saves` | Clears all save data |
| `Tools/Save Data/Open Persistent Data Path` | Opens persistent data folder |

## Code Style

- Use private fields with `_prefix` (e.g., `_playerService`)
- Use `var` only when type is obvious
- Avoid LINQ in gameplay code
- Avoid allocations in Update
- Prefer UniTask over Coroutines
- Use ScriptableObjects for configuration
- Prefer composition over inheritance
- Keep MonoBehaviours thin

## Important Conventions

- **`AddressConstants.cs` is auto-generated** — never edit manually. Run `Tools/GenerateAdresablesNames` after changing Addressable addresses.
- **Assets loaded via Addressables** — injected as `UniTask<T>` constructor params (lazy promises).
- **Services bound as `AsSingle()`** with `BindInterfacesAndSelfTo`.
- **Models are plain C#** — no MonoBehaviour inheritance, no DI dependencies.
- **UI is global** — `UIRoot` uses `DontDestroyOnLoad` across all scenes.

## Code Generation / Tooling

- `AddressConstants.cs` → generated from Addressable asset addresses (`Assets/Scripts/Editor/AddressablesNamesGenerator.cs`)
- `ViewModuleGenerator` → scaffolds View + Presenter + updates `ViewType.cs` enum + `ViewSettings` ScriptableObject
- `LevelGeneratorWindow` → editor window for generating/previewing levels with A* solver

## Gotchas

- `DestroyImmediate` is used in play mode in `CircleController.ClearCircle()` (line 145) and `StripController.ClearStrip()` (line 276) — should be `Destroy()`.
- `FindObjectOfType` called on every audio play in `AudioService.GetMonoBehaviour()` (line 193) — no caching.
- `FindObjectsByType<Camera>()` fallback in `CircleRotationService.RotateCircle()` (line 130) every frame while dragging.
- `BinaryFormatter` in `SaveDataService` (lines 84, 104) — deprecated, insecure, not supported on IL2CPP/WebGL.
- `Debug.LogError` fires in `StripController.BuildStrip()` (line 146) during normal gameplay when references are missing.
- Duplicate lose-check logic in both `CircleControllerService.CheckForLose()` (line 74) and `LevelService.CheckForLose()` (line 53).
- `AddressConstants` has a typo: `GircleModule` should be `CircleModule` (line 14) — auto-generated, fix the Addressable address.

## Key File Paths

| What | Path |
|------|------|
| Project installer | `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs` |
| Game state machine | `Assets/Feature/GameStateModule/Scripts/GameStateMachine.cs` |
| Addressable constants (generated) | `Assets/Scripts/AddressConstants.cs` |
| Level generator (editor) | `Assets/Feature/LevelModule/Scripts/Editor/Generator/LevelGeneratorWindow.cs` |
| View module generator (editor) | `Assets/Feature/UIServiceModule/Editor/ViewModuleGenerator.cs` |
| Addressable names generator (editor) | `Assets/Scripts/Editor/AddressablesNamesGenerator.cs` |
| Package manifest | `Packages/manifest.json` |
| Circle geometry docs | `Assets/Docs/CircleGeometrySystem.md` |
| Level generation docs | `Assets/Docs/LevelGenerationSystem.md` |

## Adding a New Feature Module

1. Create `Assets/Feature/<Name>Module/Scripts/` directory structure
2. Create an installer class and register it in `ProjectContextInstaller.cs`
3. For new UI views: use `Tools/UI/Create View Module` to scaffold
4. Add any new Addressable assets, then run `Tools/GenerateAdresablesNames`
