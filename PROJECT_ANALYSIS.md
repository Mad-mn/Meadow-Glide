# Color Rings - Project Analysis

## 1. Architecture Summary

### Tech Stack
- **Unity 6** with URP 2D rendering pipeline
- **Zenject** for dependency injection (project-scoped, not VContainer)
- **UniTask** for async/await (replaces coroutines in services)
- **Unity Addressables** for asset loading and instance management
- **Unity Input System** (new) for input handling
- **TextMeshPro** for UI text

### Pattern: Feature-Based Modular Architecture
The project follows a **feature folder** structure where each domain concern is isolated into its own module under `Assets/Feature/`. Each module typically contains:
- Service interfaces (`I*Service`)
- Service implementations
- Models (plain C# data holders, not MonoBehaviours)
- MonoBehaviours for view/visual components
- ScriptableObject configs
- A Zenject installer (`Installers/*ModuleInstaller.cs`)
- Presenters (MVP pattern for UI views)

### DI Container Strategy
- **ProjectContextInstaller** (ScriptableObjectInstaller) installs all 20 module installers at project scope (singleton lifetime, persist across scenes)
- **GameSceneInstaller** exists but is currently empty
- Most services are bound as `AsSingle()` with `BindInterfacesAndSelfTo`
- Assets (prefabs, configs) are loaded via Addressables and injected as `UniTask<T>` constructor parameters (lazy-loaded promises)

### Key Architectural Layers

```
┌─────────────────────────────────────────────┐
│              GameStateMachine               │
│  (BootstrapState → MainMenuState →          │
│   GameSimpleState)                          │
├─────────────────────────────────────────────┤
│           View Layer (MVP)                  │
│  ViewService manages Views + Presenters     │
│  Views loaded via Addressables              │
├─────────────────────────────────────────────┤
│           Game Logic Layer                  │
│  LevelInitializeService orchestrates level   │
│  setup: strips, slide areas, tutorial       │
├─────────────────────────────────────────────┤
│           Domain Services                   │
│  CircleRotationService, SlideSegmentService │
│  StripRotationService, MoveTrackService     │
│  CircleControllerService, SegmentStatusService│
├─────────────────────────────────────────────┤
│           Data/Config Layer                 │
│  ScriptableObjects (LevelConfig, CircleConfig│
│  CircleParamsConfig, ViewSettings)          │
│  SaveDataModel (in-memory), SaveDataService │
└─────────────────────────────────────────────┘
```

### Module Inventory (20 modules)

| Module | Responsibility |
|--------|---------------|
| AddressableModule | Addressable asset loading/caching wrapper |
| AssetBindingModule | Static asset address binding |
| Bootstrap | Project/Game scene installers |
| CameraServiceModule | Camera initialization and lifecycle |
| CircleModule | Circle rings: segments, rotation, completion |
| ColorServiceModule | Color palette mapping |
| ConfirmExitToMainMenuViewModule | Confirmation popup |
| DebugViewModule | Debug panel |
| GameStateModule | State machine (Bootstrap/MainMenu/Game) |
| GameViewModule | In-game HUD (level, moves) |
| InputModule | Input System wrapper |
| LevelInitializeModule | Level setup orchestrator |
| LevelModule | Level configs, level service |
| LoadingViewModule | Loading screen |
| LoseViewModule | Loss screen |
| MainMenuViewModule | Main menu |
| SaveDataModule | Binary save/load + in-memory model |
| SceneLoadModule | SceneManager wrapper with events |
| SettingsViewModule | Settings popup |
| SlideAreaModule | Slide interaction zones |
| SoundModule | Audio playback |
| StatusModule | Segment/area status data providers |
| StripRotationModule | Horizontal strip scrolling |
| StripsModule | Strip segments (linear representation) |
| TrackMoveModule | Move counter and cost tracking |
| TutorialModule | Tutorial state machine |
| UIServiceModule | View system (load/show/hide via Addressables) |
| VibrationModule | Haptic feedback |

---

## 2. Scene Flow Summary

### Build Scenes (in order)
1. `Assets/Scenes/InitScene.unity` — Bootstrap scene
2. `Assets/Scenes/MainMenu.unity` — Main menu
3. `Assets/Scenes/GameSimple.unity` — Gameplay

### Flow Diagram

```
InitScene (index 0)
  │
  ├─ ProjectContextInstaller loads → all 20 module installers
  ├─ GameStateMachine.Initialize() → enters BootstrapState
  │
  ▼
BootstrapState.Enter()
  ├─ SaveDataService.LoadAll()                    (sync)
  ├─ CameraService.Initialize()                   (async, DontDestroyOnLoad camera)
  ├─ ViewService.Initialize()                     (async, creates UIRoot)
  ├─ Show LoadingView
  ├─ InitializeDataProviders                      (async: segments visual, slide area, audio)
  ├─ AudioService.Initialize()                    (sync)
  ├─ VibrationService.Initialize()                (sync)
  ├─ LevelService.Initialize()                    (async: loads LevelConfigProvider)
  ├─ Prewarm MainMenuView                         (async)
  ├─ Prewarm GameView                             (async)
  └─ → MainMenuState
  │
  ▼
MainMenuState.Enter()
  ├─ Load MainMenu scene (async)
  ├─ Show MainMenuView
  ├─ Hide LoadingView
  └─ [On Exit]: Show LoadingView, Hide MainMenu,
                 Prewarm WinLevel + LoseView
  │
  ▼
GameSimpleState.Enter()
  ├─ Load GameSimple scene (async)
  ├─ [On scene loaded]:
  │   ├─ LevelInitializeService.Initialize()
  │   │   ├─ Get level config from provider
  │   │   ├─ Cache moves for level
  │   │   ├─ Show GameView
  │   │   ├─ Initialize SlideAreaService + StripSpawnService (async)
  │   │   ├─ Spawn strips from circle configs
  │   │   ├─ Spawn slide areas
  │   │   ├─ Initialize TutorialService (async)
  │   │   └─ LevelStarted() → activate tutorial, update status
  │   └─ Hide LoadingView
  └─ [On Exit]: Hide GameView, dispose level
  │
  ├─ WinLevel.ShowView  → WinLevelPresenter
  │   ├─ Next → ReloadScene (dispose → re-initialize)
  │   └─ Main Menu → MainMenuState
  │
  └─ LoseView.ShowView → LosePresenter
      ├─ Restart → ReloadScene
      ├─ Add Moves (+5) → continue playing
      └─ Main Menu → MainMenuState
```

### Key Flow Details
- **InitScene is always loaded first** but the state machine runs at project scope (DontDestroyOnLoad UIRoot + Camera)
- **Scene transitions** use `SceneLoadService` which wraps `SceneManager.LoadSceneAsync`
- **Level reload** is done via `LevelInitializeService.ReloadScene()` — disposes current level, then re-initializes without scene reload
- **UI is global**: UIRoot uses `DontDestroyOnLoad` and lives across all scenes

---

## 3. Manager Dependency Graph

### Core Singletons (Project Scope)

```
GameStateMachine
  ├─ depends on → BootstrapState, MainMenuState, GameSimpleState

BootstrapState
  ├─ IViewService, ICameraService, ISaveDataService
  ├─ ISegmentStatusVisualDataProvider, ISlideAreaDataProvider
  ├─ ILevelService, IAudioDataProvider, IAudioService, IVibrationService

MainMenuState
  ├─ ISceneLoadService, IViewService

GameSimpleState
  ├─ ISceneLoadService, ILevelInitializeService, IViewService
```

### Service Dependency Graph

```
AddressableService ← (standalone, wraps Addressables)

CameraService ← DiContainer, UniTask<Camera>

ViewService ← IAddressableService, UniTask<UIRoot>, UniTask<ViewSettings>, DiContainer, ICameraService

CircleColorService ← UniTask<CircleColorProvider>

LevelService ← UniTask<LevelConfigProvider>, ISaveDataModel, ITutorialService, ISegmentStatusService,
               LevelModel, MoveTrackModel, IViewService

LevelInitializeService ← IViewService, ILevelService, ITutorialService, MoveTrackModel,
                          IStripSpawnService, ISlideAreaService, IStripRotationService,
                          ISlideSegmentService, ICircleControllerService, StripModel

CircleRotationService ← IInputService, IInteractionStateService, GameCircleModel, MoveTrackModel,
                         ISlideSegmentService, ICameraService, IAudioService, IVibrationService

StripRotationService ← IInputService, MoveTrackModel, IInteractionStateService,
                        IAudioService, IVibrationService, StripModel, ISlideSegmentService, ICameraService

SlideSegmentService ← IInputService, IInteractionStateService, ICameraService, UniTask<CircleParamsConfig>,
                       StripModel, SlideAreaModel, MoveTrackModel, LevelModel, IAudioService, IVibrationService

CircleControllerService ← StripModel, IViewService, ISaveDataModel, ISaveDataService, MoveTrackModel

MoveTrackService ← SlideAreaModel, StripModel, MoveTrackModel, IViewService

StripSpawnService ← UniTask<StripController>, IInstantiator, UniTask<CircleParamsConfig>,
                     IStripRotationService, ISlideSegmentService, StripModel

SlideAreaService ← DiContainer, UniTask<SlideArea>, UniTask<CircleParamsConfig>,
                    ISlideAreaDataProvider, SlideAreaModel, ISlideSegmentService

AudioService ← IAudioDataProvider, DiContainer, SaveDataModel

TutorialService ← ISaveDataModel, ITutorialFactory, ITutorialAssetProvider

SaveDataService ← ISaveDataModel

SegmentStatusService ← StripModel, SlideAreaModel

CircleCompleteTrackService ← StripModel
```

### Models (Plain C# — No Dependencies)

```
GameCircleModel     — tracks circles, completion status
StripModel          — tracks strips, completion status
MoveTrackModel      — tracks moves left
LevelModel          — level start/end events
SlideAreaModel      — tracks slide areas, active segments
SaveDataModel       — in-memory dictionary of save data
```

---

## 4. Potential Performance Issues

### HIGH Priority

1. **`FindObjectOfType` in `AudioService.GetMonoBehaviour()`** (`AudioService.cs:193`)
   - Called on every `PlayMusic()`, `StopMusic()`, `FadeIn()` — finds a random MonoBehaviour to run coroutines
   - **Fix**: Cache a reference or use a dedicated MonoBehaviour host

2. **`FindObjectsByType` in `CircleRotationService.RotateCircle()`** (`CircleRotationService.cs:130`)
   - Called every `Tick()` frame while dragging a circle
   - Falls back to `GameObject.FindObjectsByType<Camera>()` when `Camera.main` is null
   - **Fix**: Inject camera reference (already available via `_cameraService.CameraObject`)

3. **`DestroyImmediate` in `CircleController.ClearCircle()`** (`CircleController.cs:143-153`)
   - `DestroyImmediate` is called in play mode — should use `Destroy` instead
   - Called when rebuilding circle segments
   - **Fix**: Replace with `Destroy()`

4. **`DestroyImmediate` in `StripController.ClearStrip()`** (`StripController.cs:282-295`)
   - Same issue as above
   - **Fix**: Replace with `Destroy()`

5. **`BinaryFormatter` in `SaveDataService`** (`SaveDataService.cs:84,104`)
   - `BinaryFormatter` is deprecated and has security vulnerabilities
   - Not supported on some platforms (IL2CPP, WebGL)
   - **Fix**: Switch to JSON (JsonUtility/Newtonsoft) or MessagePack

### MEDIUM Priority

6. **`GetSegmentAtColumn` has `Debug.LogError` calls** (`StripController.cs:85-88`)
   - Two `Debug.LogError(columnIndex)` and `Debug.LogError(slotIndex)` calls every time a segment is looked up
   - This is called during gameplay in `SlideSegmentService.UpdateSegmentsInAreas()`
   - **Fix**: Remove debug logging

7. **LINQ in hot paths** (`CircleRotationService.cs:94-96`, `SlideSegmentService.cs:96`)
   - `.OrderBy().FirstOrDefault()` called on pointer down and during slide area updates
   - Allocates on each call
   - **Fix**: Use simple loop with manual min-tracking

8. **LINQ in `AudioService.GetClip()`** (`AudioService.cs:133`)
   - `.First()` on every sound play
   - **Fix**: Use a Dictionary lookup

9. **`_circleColorProvider.Mappings.FirstOrDefault()` in `CircleColorService.GetColor()`** (`CircleColorService.cs:22`)
   - Called frequently during segment initialization
   - **Fix**: Cache a Dictionary mapping

10. **Ghost object instantiation during slide** (`SlideSegmentService.cs:198-204`)
    - `Instantiate()` called per active segment on every slide start
    - Creates temporary ghost objects that are destroyed on snap
    - Consider pooling

11. **`StripController.TrySpawnWrapGhost` instantiates per-frame during scroll** (`StripController.cs:261`)
    - Ghost objects created/destroyed during `SetScrollOffset(showWrapGhosts: true)`
    - **Fix**: Pool the ghost objects

12. **Event subscription leak risk in `GameSimpleState`** (`GameSimpleState.cs:23`)
    - `_sceneLoadService.OnSceneLoaded += OnLoadGameScene` is subscribed in `Enter()`
    - Unsubscribed in `Exit()` — but if `Enter()` is called twice without `Exit()`, the event would double-subscribe
    - **Fix**: Unsubscribe before subscribing, or guard with a flag

13. **Duplicate lose-check logic** (`CircleControllerService.CheckForLose()` and `LevelService.CheckForLose()`)
    - Both subscribe to `MoveTrackModel.OnMovesChanged` and show `LoseView`
    - Could result in the view being shown twice or conflicting state
    - **Fix**: Consolidate to one place

### LOW Priority

14. **`await UniTask.Delay(1)` in `LevelInitializeService`** (`LevelInitializeService.cs:66`)
    - 1ms delay just to allow a frame to pass — fragile
    - Consider using `await UniTask.Yield()` or `await UniTask.NextFrame()`

15. **`IsCompleted` property on `CircleController` and `StripController`** recalculates every access
    - Used in `CircleCompleteTrackService.HandleSegmentsChanged()` iterating all strips
    - **Fix**: Cache and invalidate on segment changes

16. **`ArcRenderer.UpdateArc()` with `[ExecuteAlways]`** recalculates even when nothing changed in editor
    - Minor editor performance concern

---

## 5. Potential Refactoring Opportunities

### HIGH Value

1. **Extract `IState` subscription safety** — The state machine should auto-unsubscribe `ChangeState` event on exit. Currently manual, error-prone (`GameStateMachine.cs:31`)

2. **Consolidate lose/win detection** — `CircleControllerService` and `LevelService` both check for lose conditions independently. Merge into a single `GameResultService`

3. **Replace `BinaryFormatter`** — Deprecated, insecure, platform-limited. Switch to `JsonUtility` or a custom serializer

4. **Remove debug logging in production code** — `StripController.GetSegmentAtColumn()` has `Debug.LogError` calls that appear to be leftover debugging (`StripController.cs:85,88`)

5. **Fix `DestroyImmediate` in play mode** — Replace with `Destroy` in `CircleController.ClearCircle()` and `StripController.ClearStrip()`

### MEDIUM Value

6. **Cache camera reference** — `CircleRotationService` has `_cameraService` but falls back to `Camera.main` / `FindObjectsByType`. Use `_cameraService.CameraObject` consistently

7. **Replace LINQ lookups with Dictionary caches** — `CircleColorService.GetColor()`, `AudioService.GetClip()`, `SlideSegmentService.GetStripByIndex()`

8. **Pool ghost segments** — Both `SlideSegmentService` and `StripController` create/destroy ghost objects frequently. Implement a simple object pool

9. **`AudioService` coroutine host** — Create a dedicated `MonoBehaviour` singleton instead of `FindObjectOfType<MonoBehaviour>()` every time

10. **`ViewService.CreatePresenter()` reflection** — Uses `Type.GetType()` + assembly scanning every time a presenter is created. Cache the type lookup

11. **Simplify `LevelInitializeService` constructor** — Takes 10 dependencies. Consider grouping related services into a facade

12. **`StripController` is too large** (301 lines) — Mixes spawning, layout, ghost management, and segment manipulation. Consider extracting ghost/wrap logic

13. **`SlideSegmentService` is very large** (466 lines) — Handles input, visuals, snap animation, and segment management. Consider splitting into:
    - `SlideInputHandler` (input detection)
    - `SlideVisualController` (ghost/visual updates)
    - `SlideSnapService` (snap animation)

### LOW Value

14. **`GameCircleModel` is unused?** — Referenced by `CircleRegistrator` and `CircleRotationService`, but the `CircleControllerService` uses `StripModel` instead. Verify if `GameCircleModel` is still needed or if it's dead code from an earlier architecture

15. **`CircleCompleteTrackService` name mismatch** — References `_stripModel` (not circle model). The class name suggests circle completion but works with strips

16. **`ISaveData` marker interface** — Currently empty. Could add a `Version` field for migration support

17. **Tutorial completion not persisted** — `TutorialService.IsTutorialCompleted` always returns `false`. Need save data integration

18. **`AddressConstants` has typo** — `GircleModule` should be `CircleModule` (`AddressConstants.cs:13`)

19. **`ArcRenderer` not used in game?** — Only referenced in `Assets/Scripts/`. Verify if it's dead code or used somewhere

20. **Event naming inconsistency** — `CircleRotationStatusChanges` vs `OnStripCompletedStatusChanged` vs `OnSegmentsChanged`. Standardize to `On*` prefix for events

---

## Summary Statistics

| Metric | Count |
|--------|-------|
| Feature Modules | ~20+ |
| C# Scripts (project) | ~100+ |
| Scenes | 3 (InitScene, MainMenu, GameSimple) |
| ScriptableObject Assets | ~50+ (level configs, balance configs) |
| Level Configs | 20 (GeneratedLevels/Level_1 through Level_20) |
| Addressable Groups | 4 (Prefabs, UI, Configurations, unifiedraytracing) |
| View Types | 9 (MainMenu, Settings, Game, Loading, Win, Lose, ConfirmExit, Debug, Settings) |
| Game States | 3 (Bootstrap, MainMenu, GameSimple) |
