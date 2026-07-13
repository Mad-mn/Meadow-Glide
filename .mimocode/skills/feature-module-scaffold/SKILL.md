---
name: feature-module-scaffold
description: Scaffold a new feature module for the Color Rings Unity project following the established Zenject + UniTask + Addressables pattern. Creates directory structure, service interface/implementation, installer, and registers in ProjectContextInstaller.
---

# Feature Module Scaffolding

Create a new feature module following the project's established conventions. This skill covers the complete boilerplate: directory structure, service binding, installer registration, and optional Addressable config provider wiring.

## When to use

- User asks to create a new feature/system/module
- User describes functionality that needs its own service, model, or config
- Creating a new gameplay system (e.g., HintModule, ChallengeModule, UndoModule)

## When NOT to use

- Adding a new UI view → use `Tools/UI/Create View Module` editor tool instead
- Modifying an existing module → just edit the files directly
- One-off scripts or utilities that don't need DI

## Prerequisites

- AGENTS.md is up to date (check module count)
- Project uses Zenject (vendored at `Assets/Plugins/Zenject/`)
- UniTask for async (`Cysharp.Threading.Tasks`)
- Addressables for asset loading

## Step-by-step procedure

### 1. Create directory structure

```
Assets/Feature/<Name>Module/
  Scripts/
    Installers/
      <Name>ModuleInstaller.cs
    I<Name>Service.cs
    <Name>Service.cs
    (optional) <Name>Model.cs
    (optional) <Name>Config.cs        — ScriptableObject if config needed
    (optional) <Name>Configs.cs       — wrapper SO holding array of configs
    (optional) I<Name>ConfigProvider.cs
    (optional) <Name>ConfigProvider.cs
```

**Naming convention**: Module name = `<Name>Module`. Namespace = `Feature.<Name>Module.Scripts`. Installer namespace = `Feature.<Name>Module.Scripts.Installers`.

### 2. Create service interface

File: `I<Name>Service.cs`

```csharp
namespace Feature.<Name>Module.Scripts {
    public interface I<Name>Service {
        // Public API — keep minimal, future-proof
    }
}
```

Conventions:
- Interface name = `I<Name>Service`
- No MonoBehaviour inheritance
- No Zenject attributes on interface
- Use UniTask for async methods

### 3. Create service implementation

File: `<Name>Service.cs`

```csharp
namespace Feature.<Name>Module.Scripts {
    public class <Name>Service : I<Name>Service {
        // Implementation
    }
}
```

Conventions:
- Private fields with `_prefix` (e.g., `_otherService`)
- Inject dependencies via constructor (Zenject `[Inject]` or implicit constructor injection)
- No `MonoBehaviour` inheritance — plain C# class
- Bind as `AsSingle()` in installer

### 4. Create Zenject installer

File: `Scripts/Installers/<Name>ModuleInstaller.cs`

```csharp
using Zenject;

namespace Feature.<Name>Module.Scripts.Installers {
    public class <Name>ModuleInstaller : Installer<<Name>ModuleInstaller> {
        public override void InstallBindings() {
            Container.BindInterfacesAndSelfTo<<Name>Service>()
                .AsSingle();

            // If config provider exists:
            // Container.BindInterfacesAndSelfTo<<Name>ConfigProvider>()
            //     .AsSingle();
        }
    }
}
```

Conventions:
- Extends `Installer<T>` (not `Installer` without generic)
- Use `BindInterfacesAndSelfTo` + `AsSingle()` for services
- Use `BindInterfacesTo` if self-binding not needed

### 5. Register in ProjectContextInstaller

File: `Assets/Feature/Bootstrap/Scripts/ProjectContextInstaller.cs`

1. Add using directive at top:
```csharp
using Feature.<Name>Module.Scripts.Installers;
```

2. Add install call inside `InstallBindings()`:
```csharp
<Name>ModuleInstaller.Install(Container);
```

**Placement**: Add after related modules (e.g., if module depends on TrackMoveModule, place after it). Keep alphabetical-ish ordering within logical groups.

### 6. (If config needed) Create ScriptableObject config

File: `Scripts/<Name>Config.cs`

```csharp
using UnityEngine;

namespace Feature.<Name>Module.Scripts {
    [CreateAssetMenu(fileName = "<Name>Config", menuName = "Configs/<Name>/<Name>Config")]
    public class <Name>Config : ScriptableObject {
        // Config fields
    }
}
```

### 7. (If config needed) Create config provider

File: `Scripts/<Name>ConfigProvider.cs`

```csharp
using System.Collections.Generic;
using Cysharp.Threading.Tasks;

namespace Feature.<Name>Module.Scripts {
    public class <Name>ConfigProvider : I<Name>ConfigProvider {
        private readonly UniTask<<Name>Configs> _configsTask;
        private Dictionary<KeyType, <Name>Config> _configsByKey;

        public <Name>ConfigProvider(UniTask<<Name>Configs> configsTask) {
            _configsTask = configsTask;
        }

        public async UniTask Initialize() {
            <Name>Configs configs = await _configsTask;
            _configsByKey = new Dictionary<KeyType, <Name>Config>();
            foreach (<Name>Config config in configs.Configs) {
                if (config != null) {
                    _configsByKey[config.Key] = config;
                }
            }
        }

        public <Name>Config GetConfig(KeyType key) {
            if (_configsByKey != null && _configsByKey.TryGetValue(key, out <Name>Config config)) {
                return config;
            }
            return null;
        }
    }
}
```

### 8. (If config needed) Bind Addressable asset

File: `Assets/Feature/AssetBindingModule/Scripts/Installers/AssetBindingModuleInstaller.cs`

Add:
```csharp
Container.BindAddressableAsset<<Name>Configs>(AddressConstants.<Name>Configs);
```

### 9. (If config needed) Run Addressables generator

After creating Addressable assets in Unity Editor, run:
`Tools > GenerateAdresablesNames`

This regenerates `Assets/Scripts/AddressConstants.cs`.

### 10. Update AGENTS.md

Update the module count and add the new module to the relevant section.

## Config provider pattern reference

This exact pattern is used by `TransactionConfigsProvider`, `ChallengeConfigProvider`, `PerfectMapRewardConfigProvider`:

1. Config SO holds array of config entries
2. Provider takes `UniTask<ConfigSO>` in constructor (lazy Addressables load)
3. `Initialize()` awaits the task and builds a dictionary
4. `GetConfig(key)` does dictionary lookup
5. Bound in module installer with `BindInterfacesAndSelfTo<Provider>().AsSingle()`

## Example modules (reference)

| Module | Complexity | Has Config | Has Model |
|--------|-----------|------------|-----------|
| MoveEfficiencyModule | Minimal (4 files) | No | No |
| ChallengeModule | Medium (10 files) | Yes | Yes |
| TransactionModule | Complex (15 files) | Yes | Yes |
| UndoModule | Medium | No | Yes |

## Validation

After creating the module:
1. Check that the installer compiles (no missing types)
2. Verify `ProjectContextInstaller` has the new using + install call
3. If Addressable assets were created, verify `AddressConstants.cs` was regenerated
4. Check that the service can be injected into other services that depend on it
