# Player Inventory / Progression Save System

## Overview

Introduce a dedicated `PlayerInventoryModule` that manages persistent player resources (coins today, boosters/currencies later) independently from level progress saves. The system uses a dictionary-based save model that is inherently extensible to future resource types.

---

## Architecture

### New Types

| Type | File | Purpose |
|------|------|---------|
| `ResourceType` enum | `Assets/Feature/PlayerInventoryModule/Scripts/ResourceType.cs` | Identifies resource kinds (Coins=1, future: Hint=2, etc.) |
| `PlayerInventoryData` | `Assets/Feature/SaveDataModule/Scripts/SavedData/PlayerInventoryData.cs` | Serializable save model: `Dictionary<ResourceType, int>` balances |
| `IPlayerInventoryService` | `Assets/Feature/PlayerInventoryModule/Scripts/IPlayerInventoryService.cs` | Public API for resource operations |
| `PlayerInventoryService` | `Assets/Feature/PlayerInventoryModule/Scripts/PlayerInventoryService.cs` | Implementation: reads/writes via SaveDataModel, enforces invariants |
| `PlayerInventoryModel` | `Assets/Feature/PlayerInventoryModule/Scripts/PlayerInventoryModel.cs` | In-memory state + events (OnBalanceChanged) |
| `PlayerInventoryModuleInstaller` | `Assets/Feature/PlayerInventoryModule/Scripts/Installers/PlayerInventoryModuleInstaller.cs` | Zenject bindings |

### Modified Types

| File | Change |
|------|--------|
| `Assets/Feature/SaveDataModule/Scripts/SaveDataType.cs` | Add `PlayerInventory = 4` to enum |
| `Assets/Feature/SaveDataModule/Scripts/SaveDataService.cs` | Add `PlayerInventoryData` loading in `LoadAll()` |
| `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs` | Register `PlayerInventoryModuleInstaller` |
| `Assets/Feature/CircleModule/Scripts/CircleControllerService.cs` | Use `IPlayerInventoryService.Add()` instead of direct mutation |

---

## Detailed Design

### 1. ResourceType Enum

```csharp
namespace Feature.PlayerInventoryModule.Scripts {
    public enum ResourceType {
        Coins = 1,
        // Future:
        // Hint = 2,
        // ExtraMoves = 3,
        // Undo = 4,
    }
}
```

Adding a new resource = adding one enum value. No other code changes needed in the inventory system itself.

### 2. PlayerInventoryData (Save Model)

```csharp
[Serializable]
public class PlayerInventoryData : ISaveData {
    // Dictionary keyed by ResourceType, values are amounts
    // Serialized by BinaryFormatter
    public Dictionary<ResourceType, int> Balances = new Dictionary<ResourceType, int>();
}
```

- Lives in `Assets/Feature/SaveDataModule/Scripts/SavedData/` alongside existing data classes
- Implements `ISaveData` (marker interface for the save system)
- `[Serializable]` for BinaryFormatter support
- Dictionary approach means new resource types require zero structural changes

### 3. SaveDataType Addition

```csharp
public enum SaveDataType {
    PlayerProgress = 1,
    Settings = 2,
    Statistics = 3,
    PlayerInventory = 4  // NEW
}
```

### 4. SaveDataService.LoadAll() Update

Add one line to `LoadAll()`:

```csharp
_model.Set(SaveDataType.PlayerInventory,
    LoadFromDisk<PlayerInventoryData>(SaveDataType.PlayerInventory));
```

This loads the inventory from disk into the in-memory dictionary at bootstrap.

### 5. PlayerInventoryModel (In-Memory State)

```csharp
public class PlayerInventoryModel {
    public event Action<ResourceType, int> OnBalanceChanged;
    public bool IsLoaded { get; private set; }

    private Dictionary<ResourceType, int> _balances = new Dictionary<ResourceType, int>();

    public int GetBalance(ResourceType type) {
        return _balances.TryGetValue(type, out var amount) ? amount : 0;
    }

    public void SetBalance(ResourceType type, int amount) {
        _balances[type] = amount;
        OnBalanceChanged?.Invoke(type, amount);
    }

    public Dictionary<ResourceType, int> GetAll() => new Dictionary<ResourceType, int>(_balances);

    public void LoadFrom(Dictionary<ResourceType, int> source) {
        _balances = new Dictionary<ResourceType, int>(source);
        IsLoaded = true;
    }
}
```

- Plain C# class, no MonoBehaviour
- Fires `OnBalanceChanged` event whenever a balance changes (for UI binding)
- Acts as the single source of truth at runtime

### 6. IPlayerInventoryService Interface

```csharp
public interface IPlayerInventoryService {
    int GetBalance(ResourceType type);
    bool HasEnough(ResourceType type, int amount);
    bool TrySpend(ResourceType type, int amount);
    void Add(ResourceType type, int amount);
}
```

### 7. PlayerInventoryService Implementation

```csharp
public class PlayerInventoryService : IPlayerInventoryService {
    private readonly PlayerInventoryModel _model;
    private readonly ISaveDataModel _saveDataModel;
    private readonly ISaveDataService _saveDataService;

    public PlayerInventoryService(
        PlayerInventoryModel model,
        ISaveDataModel saveDataModel,
        ISaveDataService saveDataService) {
        _model = model;
        _saveDataModel = saveDataModel;
        _saveDataService = saveDataService;
    }

    // No IInitializable — SaveDataService.LoadAll() already populates SaveDataModel
    // with PlayerInventoryData. The model is loaded on first GetBalance() call.
    private void EnsureLoaded() {
        if (_model.IsLoaded) return;
        var data = _saveDataModel.Get<PlayerInventoryData>(SaveDataType.PlayerInventory);
        _model.LoadFrom(data.Balances);
    }

    public int GetBalance(ResourceType type) {
        EnsureLoaded();
        return _model.GetBalance(type);
    }

    public bool HasEnough(ResourceType type, int amount) {
        EnsureLoaded();
        return _model.GetBalance(type) >= amount;
    }

    public bool TrySpend(ResourceType type, int amount) {
        EnsureLoaded();
        if (!HasEnough(type, amount)) return false;
        var current = _model.GetBalance(type);
        _model.SetBalance(type, current - amount);
        Persist();
        return true;
    }

    public void Add(ResourceType type, int amount) {
        EnsureLoaded();
        var current = _model.GetBalance(type);
        _model.SetBalance(type, current + amount);
        Persist();
    }

    private void Persist() {
        var data = _saveDataModel.Get<PlayerInventoryData>(SaveDataType.PlayerInventory);
        data.Balances = _model.GetAll();
        _saveDataService.Save(SaveDataType.PlayerInventory);
    }
}
```

**Key design decisions:**
- `TrySpend` returns bool (fails gracefully if insufficient) — prevents external balance checks
- `Add` always persists immediately — no deferred saves
- `Persist()` syncs model state back to save data model, then flushes to disk
- All balance mutations go through the service — consumers never touch `PlayerInventoryData` directly

### 8. PlayerInventoryModuleInstaller

```csharp
public class PlayerInventoryModuleInstaller : Installer<PlayerInventoryModuleInstaller> {
    public override void InstallBindings() {
        Container.BindInterfacesAndSelfTo<PlayerInventoryModel>().AsSingle();
        Container.BindInterfacesAndSelfTo<PlayerInventoryService>().AsSingle();
    }
}
```

### 9. ProjectContextInstaller Registration

Add to `InstallBindings()`:

```csharp
PlayerInventoryModuleInstaller.Install(Container);
```

Also register EconomyConfig in `AssetBindingModuleInstaller`:

```csharp
Container.BindAddressableAsset<EconomyConfig>(AddressConstants.EconomyConfig);
```

### 10. CircleControllerService Integration

Replace direct `PlayerProgressData.Level++` mutation pattern in `ApplyWin()`:

```csharp
// BEFORE:
_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level++;
_saveDataService.Save(SaveDataType.PlayerProgress);

// AFTER (coins addition added alongside existing level save):
_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level++;
_saveDataService.Save(SaveDataType.PlayerProgress);

_inventoryService.Add(ResourceType.Coins, _economyConfig.LevelWinReward);
```

The `PlayerInventoryService` is injected via constructor. The `_inventoryService.Add()` call handles persistence internally.

### 11. Economy Config (avoid hardcoding)

Create `Assets/Feature/PlayerInventoryModule/Configs/EconomyConfig.cs`:

```csharp
[CreateAssetMenu(fileName = "EconomyConfig", menuName = "Configs/EconomyConfig")]
public class EconomyConfig : ScriptableObject {
    public int LevelWinReward = 50;
    // Future: HintCost, ExtraMovesCost, UndoCost, etc.
}
```

Load via Addressables and inject as `UniTask<EconomyConfig>` (follows existing pattern from LevelConfigProvider).

---

## File Change Summary

| # | File | Action |
|---|------|--------|
| 1 | `Assets/Feature/PlayerInventoryModule/Scripts/ResourceType.cs` | CREATE |
| 2 | `Assets/Feature/PlayerInventoryModule/Scripts/IPlayerInventoryService.cs` | CREATE |
| 3 | `Assets/Feature/PlayerInventoryModule/Scripts/PlayerInventoryModel.cs` | CREATE |
| 4 | `Assets/Feature/PlayerInventoryModule/Scripts/PlayerInventoryService.cs` | CREATE |
| 5 | `Assets/Feature/PlayerInventoryModule/Scripts/Installers/PlayerInventoryModuleInstaller.cs` | CREATE |
| 5b | `Assets/Feature/PlayerInventoryModule/Configs/EconomyConfig.cs` | CREATE |
| 5c | `Assets/Feature/AssetBindingModule/Scripts/AssetBindingModuleInstaller.cs` | EDIT (register EconomyConfig) |
| 6 | `Assets/Feature/SaveDataModule/Scripts/SaveDataType.cs` | EDIT (add enum value) |
| 7 | `Assets/Feature/SaveDataModule/Scripts/SavedData/PlayerInventoryData.cs` | CREATE (new file, separate from PlayerProgressData) |
| 8 | `Assets/Feature/SaveDataModule/Scripts/SaveDataService.cs` | EDIT (add load in LoadAll) |
| 9 | `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs` | EDIT (register installer) |
| 10 | `Assets/Feature/CircleModule/Scripts/CircleControllerService.cs` | EDIT (inject + call inventory service) |

---

## How Future Resources Are Added

1. Add enum value to `ResourceType` (e.g., `Hint = 2`)
2. Use the existing API: `_inventoryService.Add(ResourceType.Hint, 1)` or `_inventoryService.TrySpend(ResourceType.Hint, 1)`
3. No changes to save system, model, or service needed

---

## How Direct Mutation Is Prevented

- `PlayerInventoryData.Balances` is a public field (required for BinaryFormatter serialization) but is only accessed by `PlayerInventoryService.Persist()`
- All consumers inject `IPlayerInventoryService`, not `PlayerInventoryData`
- The service enforces invariants (no negative balances via `TrySpend`)
- Events propagate balance changes to UI without exposing internals

---

## Initial Balance

New and existing players both start with **0 coins**. No migration bonus. `PlayerInventoryData` defaults to an empty dictionary; `GetBalance()` returns 0 for missing keys.

## Verification

1. **Compile check**: Build the project — no compile errors
2. **Save/load test**: Launch game, verify `playerinventory_save.dat` is created in persistent data path
3. **Coin award test**: Complete a level, verify coin balance increases by 50
4. **Persistence test**: Close and relaunch game, verify coin balance persists
5. **Independence test**: Modify level progress, verify inventory save is unaffected
6. **Editor tools**: Use `Tools/Save Data/Open Persistent Data Path` to inspect the new `.dat` file exists
