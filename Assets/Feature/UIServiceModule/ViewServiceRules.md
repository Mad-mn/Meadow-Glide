# UI ViewService (MVP) Rules & Guidelines

This document defines the rules for using the `ViewService` and the MVP (Model-View-Presenter) pattern within this project.

## Architecture Overview
The UI system uses a decoupled **MVP** pattern managed by a centralized **ViewService**.
- **View**: Unity Component (`ViewBase`) responsible ONLY for UI elements (images, buttons, animations).
- **Presenter**: C# Class (`PresenterBase`) responsible for logic, subscribing to View events, and interacting with other services/models.
- **ViewService**: Manages lifecycle, instantiation via Addressables, and dependency injection via Zenject.

## 1. Creating a New View
1. Create a class inheriting from `ViewBase`.
2. Add serialized fields for UI components (e.g., `UnityEngine.UI.Button`).
3. Place the script in the feature's UI folder.
4. Create a Prefab and add the View component to its root.
5. Mark the Prefab as **Addressable**.

## 2. Creating a New Presenter
1. Create a class inheriting from `PresenterBase<TView>`.
2. Implement `Initialize()` to subscribe to View events and service signals.
3. Implement `Dispose()` to unsubscribe and clean up.
4. Presenters are instantiated by the `ViewService` using Zenject's `DiContainer.Instantiate`.

## 3. Configuration (ViewSettings)
All views must be registered in the `ViewSettings` ScriptableObject:
- **View Type**: Enum value from `ViewType`.
- **Address**: The Addressables key for the prefab.
- **Presenter Type Name**: Full class name (including namespace) of the Presenter.

## 4. Usage in Code
Inject `IViewService` and use the following methods:

### Showing a View
```csharp
// Returns the View component instance. Presenter is created automatically.
var mainView = await _viewService.ShowView<MainMenuView>(ViewType.MainMenu);
```

### Hiding a View
```csharp
_viewService.HideView(ViewType.MainMenu);
```

## 5. Lifecycle Rules
- **DestroyOnClose**: If `_destroyOnClose` is `true` in the View, the object is destroyed and the Presenter is disposed when `HideView` is called.
- **Dependency Injection**: Use `[Inject]` in the View only for UI-specific dependencies. Business logic dependencies should be injected into the **Presenter** constructor.
- **UIRoot**: All views are parented to `UIRoot.CanvasRoot` upon instantiation.

## 6. Constraints
- Never reference the Presenter from the View.
- Never put business logic in the View.
## 7. Automation Tools
### View Module Generator
Use the editor tool via **Tools -> UI -> Create View Module**.
- It creates the folder structure: `Assets/Feature/[Name]Module`.
- It generates the `View` and `Presenter` scripts with proper inheritance and namespaces.
- **New**: It automatically adds a new entry to the `ViewSettings` asset with the correct `PresenterTypeName` and a default `Address` (matching the view name).
- **Important**: After generation, you must manually add the new view to the `ViewType` enum and then select it in the `ViewSettings` asset entry.

