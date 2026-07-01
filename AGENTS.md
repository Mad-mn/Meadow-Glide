# AGENTS.md — Color Rings Unity Project

## Project Overview

Unity 6 (6000.3.10f1) puzzle game — rotate and slide colored ring segments to match colors. URP 2D rendering. Target: Android/iOS.

## Tech Stack

- **Zenject** for DI (project-scoped via `ProjectContextInstaller` → `ScriptableObjectInstaller`)
- **UniTask** for async/await (replaces coroutines in services)
- **DOTween** for animations (via `Assets/Plugins/Demigiant/DOTween/`, not UPM)
- **Unity Addressables** for asset loading
- **Unity Input System** (new) for input
- **TextMeshPro** for UI text
- **Firebase** for analytics/remote config

## Architecture

Feature-based modular layout. Each module under `Assets/Feature/<ModuleName>/` contains:
- Service interfaces (`I*Service`) and implementations
- Models (plain C# data holders, not MonoBehaviours)
- MonoBehaviours for view/visual components
- ScriptableObject configs
- A Zenject installer (`Installers/*ModuleInstaller.cs`)
- Presenters (MVP pattern for UI views)

**37 installer calls** are registered in `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs`.

### Scenes (build order)
1. `Assets/Scenes/InitScene.unity` — Bootstrap (loads first, runs `GameStateMachine`)
2. `Assets/Scenes/MainMenu.unity` — Main menu
3. `Assets/Scenes/GameSimple.unity` — Gameplay

### State Flow
`BootstrapState` → `MainMenuState` → `GameSimpleState`

### Level Data
- `Assets/ScriptableObjects/Levels/` — hand-crafted level configs (newBalance)
- `Assets/ScriptableObjects/GeneratedLevels/` — procedurally generated levels (A* solver)
- Level configs loaded via Addressables as `UniTask<LevelConfigProvider>` (lazy)

## Editor Tools

| Menu Path | Purpose |
|-----------|---------|
| `Tools/GenerateAdresablesNames` | Regenerates `Assets/Scripts/AddressConstants.cs` from Addressable addresses |
| `Tools/ColorRings/Level Generator` | Level designer with A* solver and difficulty rating |
| `Tools/UI/Create View Module` | Scaffolds a new MVP View module (View + Presenter + updates `ViewType.cs` enum + `ViewSettings` ScriptableObject) |
| `Tools/Save Data/Clear All Saves` | Clears all save data |
| `Tools/Save Data/Open Persistent Data Path` | Opens persistent data folder |
| `Tools/Automatic UI Anchoring/Anchor Selected UI Objects` | Quick-anchors selected UI elements (F1 shortcut) |
| `Tools/Localization/Load Localization` | Import localized strings from Google Sheets CSV |
| `Tools/Localization/Validate Localization` | Check localization key consistency |

## Project Constraints

- **No CI/CD** — builds are manual via Unity Editor. No automated pipelines, no test scripts.
- **No project tests** — `com.unity.test-framework` is installed but unused. Only Zenject plugin tests exist.
- **Monolithic assembly** — no project-specific `.asmdef` files. All project code compiles into `Assembly-CSharp`.
- **Target platforms** — Android and iOS (mobile-first performance).

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

## UI View System (MVP)

Detailed rules in `Assets/Feature/UIServiceModule/ViewServiceRules.md`. Key points:
- Views inherit from `ViewBase`, Presenters from `PresenterBase<TView>`
- All views registered in `ViewSettings` ScriptableObject with ViewType enum
- Use `_viewService.ShowView<T>(ViewType)` / `_viewService.HideView(ViewType)`
- View creation and Presenter lifecycle managed by `ViewService`
- Business logic goes in Presenter, UI logic stays in View

## Code Generation

- `AddressConstants.cs` → generated from Addressable asset addresses (`Assets/Scripts/Editor/AddressablesNamesGenerator.cs`)
- `ViewModuleGenerator` → scaffolds View + Presenter + updates `ViewType.cs` enum + `ViewSettings` ScriptableObject
- `LevelGeneratorWindow` → editor window for generating/previewing levels with A* solver

## Gotchas

- `DestroyImmediate` used in play mode in both `CircleController.ClearCircle()` (line 145) and `StripController.ClearStrip()` (line 276) — should be `Destroy()`.
- `FindObjectOfType<MonoBehaviour>()` called on every audio operation in `AudioService.GetMonoBehaviour()` (line 193) — no caching.
- `FindObjectsByType<Camera>()` fallback in `CircleRotationService.RotateCircle()` (line 130) runs every frame during drag.
- `BinaryFormatter` in `SaveDataService.MigrateLegacySave()` — retained only for one-time migration from legacy `.dat` files. New saves use JSON via Newtonsoft.Json.
- LINQ allocates in hot paths: `OrderBy().FirstOrDefault()` in `CircleRotationService` (line 94) on every pointer down, and `FirstOrDefault()` in `SlideSegmentService` (lines 105, 177) on segment updates.
- `AddressConstants` has a typo: `GircleModule` should be `CircleModule` (line 21) — auto-generated, fix the Addressable address.

## Adding a New Feature Module

1. Create `Assets/Feature/<Name>Module/Scripts/` directory structure
2. Create an installer class and register it in `ProjectContextInstaller.cs`
3. For new UI views: use `Tools/UI/Create View Module` to scaffold — this also updates `ViewType` enum and `ViewSettings` ScriptableObject
4. Add any new Addressable assets, then run `Tools/GenerateAdresablesNames`

## Further Reference

- `PROJECT_ANALYSIS.md` — detailed architecture diagrams, dependency graphs, and performance/refactoring notes for the full codebase.
