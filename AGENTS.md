# AGENTS.md — Color Rings Unity Project

## Project Overview

Unity 6 (6000.3.10f1) puzzle game — rotate and slide colored ring segments to match colors. URP 2D rendering. Target: Android/iOS.

## Tech Stack

- **Zenject** for DI (project-scoped via `ProjectContextInstaller` → `ScriptableObjectInstaller`)
- **UniTask** for async/await in services (coroutines still used in animation/tutorial code)
- **DOTween** for animations (via `Assets/Plugins/Demigiant/DOTween/`, not UPM)
- **Unity Addressables** for asset loading
- **Unity Input System** (new) for input
- **TextMeshPro** for UI text
- **Firebase** for analytics/remote config
- **Unity Mobile Notifications** for local push notifications (Android)

## Architecture

Feature-based modular layout. **55 modules** under `Assets/Feature/<ModuleName>/`, each containing:
- Service interfaces (`I*Service`) and implementations
- Models (plain C# data holders, not MonoBehaviours)
- MonoBehaviours for view/visual components
- ScriptableObject configs
- A Zenject installer (`Installers/*ModuleInstaller.cs`)
- Presenters (MVP pattern for UI views)

**41 installer calls** (40 non-editor + 1 editor-only `AdRecordingModuleInstaller`) are registered in `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs`. Notable newer modules: `ChallengeModule`, `UndoModule`, `PlayerInventoryModule`, `TransactionModule`, `DailyChallengeStartViewModule`, `LocalizationModule`, `NotificationModule`.

13 view-only modules have no installer — they're loaded via Addressables and wired by `ViewService`: `BackgroundViewModule`, `ConfirmBuyViewModule`, `ConfirmExitToMainMenuViewModule`, `DailyChallengeCompleteViewModule`, `DebugViewModule`, `GameViewModule`, `LoadingViewModule`, `LoseViewModule`, `MainMenuViewModule`, `MainTutorialViewModule`, `SettingsViewModule`, `ToolButtonViewModule`, `TutorialViewModule`.

### Scenes (build order)
1. `Assets/Scenes/InitScene.unity` — Bootstrap (loads first, runs `GameStateMachine`)
2. `Assets/Scenes/MainMenu.unity` — Main menu
3. `Assets/Scenes/GameSimple.unity` — Gameplay

### State Flow
`BootstrapState` → `MainMenuState` → `GameSimpleState`

### Scene Installers
- `GameSceneInstaller.cs` exists at `Assets/Feature/Bootstrap/Scripts/` but is currently empty (no-op).

### Level Data
- `Assets/ScriptableObjects/Levels/` — 153 hand-crafted level configs (loaded via Addressables as `UniTask<LevelConfigProvider>`)
- `Assets/ScriptableObjects/DayliChallangeLevels/` — 35 daily challenge level configs (note: typo "DayliChallange" is in the actual directory name)
- Level generator tool saves to `Assets/ScriptableObjects/Levels/` — no separate generated directory

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
| `Tools/Ad Recording/Create Config` | Creates AdRecordingConfig in Resources folder |

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
- All views registered in `ViewSettings` ScriptableObject with ViewType enum (18 view types)
- Use `_viewService.ShowView<T>(ViewType)` / `_viewService.HideView(ViewType)`
- View creation and Presenter lifecycle managed by `ViewService`
- Business logic goes in Presenter, UI logic stays in View

## Code Generation

- `AddressConstants.cs` → generated from Addressable asset addresses (`Assets/Scripts/Editor/AddressablesNamesGenerator.cs`)
- `ViewModuleGenerator` → scaffolds View + Presenter + updates `ViewType.cs` enum + `ViewSettings` ScriptableObject
- `LevelGeneratorWindow` → editor window for generating/previewing levels with A* solver

## Gotchas

- `DestroyImmediate` used in play mode in `CircleController.ClearCircle()` and `StripController.ClearStrip()` — should be `Destroy()`.
- `FindObjectOfType<MonoBehaviour>()` called on every audio operation in `AudioService.GetMonoBehaviour()` — no caching.
- `FindObjectsByType<Camera>()` fallback in `CircleRotationService.RotateCircle()` runs every frame during drag.
- `SaveDataService` has an unused `using System.Runtime.Serialization.Formatters.Binary;` import — BinaryFormatter was replaced by Newtonsoft.Json. Clean it up if you touch the file.
- LINQ allocates in hot paths: `OrderBy().FirstOrDefault()` in `CircleRotationService` and `StripRotationService` on every pointer down, and `FirstOrDefault()` in `SlideSegmentService` on segment updates.
- `AddressConstants` has a typo: `GircleModule` should be `CircleModule` — auto-generated, fix the Addressable address.
- Coroutines are still actively used in `TutorialModule` hint states, `AudioService` fade logic, and animation controllers (`StripAnimationController`, `CircleAnimationController`, `SlideAreaAnimationController`). `CoroutineRunnerModule` exists as a dedicated host for coroutine execution.

## Adding a New Feature Module

1. Create `Assets/Feature/<Name>Module/Scripts/` directory structure
2. Create an installer class and register it in `ProjectContextInstaller.cs`
3. For new UI views: use `Tools/UI/Create View Module` to scaffold — this also updates `ViewType` enum and `ViewSettings` ScriptableObject
4. Add any new Addressable assets, then run `Tools/GenerateAdresablesNames`

## Further Reference

- `PROJECT_ANALYSIS.md` — detailed architecture diagrams, dependency graphs, and performance/refactoring notes for the full codebase. Note: some figures are stale (e.g., says "20 module installers" — actual count is 39).
