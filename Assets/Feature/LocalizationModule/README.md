# Localization Module

Standalone, scalable Unity localization system with Google Sheets integration.

## Features

- Enum-based localization keys
- Enum-based language selection
- CSV-based storage (generated from Google Sheets)
- O(1) lookup performance
- Automatic language detection
- Event-driven language switching
- Editor tools for import/validation

## Architecture

```
LocalizationModule/
├── Scripts/
│   ├── Data/
│   │   ├── Language.cs              # Language enum
│   │   ├── LocalizationKey.cs       # Localization keys enum
│   │   ├── LocalizationEntry.cs     # Key-value pair struct
│   │   └── LocalizationLanguageData.cs
│   ├── Installers/
│   │   └── LocalizationModuleInstaller.cs
│   ├── UI/
│   │   └── LocalizedText.cs         # UI component
│   ├── Utils/
│   │   └── CsvParser.cs             # CSV parsing utilities
│   ├── ILocalizationService.cs      # Service interface
│   ├── ILocalizationDatabase.cs     # Database interface
│   ├── LocalizationService.cs       # Core service
│   ├── LocalizationDatabase.cs      # Data storage
│   ├── LocalizationEvents.cs        # Event system
│   ├── LocalizationSettings.cs      # Configuration
│   ├── LocalizationBootstrap.cs     # Initialization
│   └── Loc.cs                       # Static helper
├── Editor/
│   ├── LocalizationImporterWindow.cs # Google Sheets importer
│   └── LocalizationValidator.cs      # Validation tools
├── Resources/
│   └── Localization/
│       └── Localization.csv         # Localized strings
└── Docs/
    └── ReverseSyncAnalysis.md       # Sync analysis
```

## Setup

### 1. Add to Project

1. Copy `LocalizationModule` folder to `Assets/Feature/`
2. Register installer in `ProjectContextInstaller.cs`:
   ```csharp
   LocalizationModuleInstaller.Install(Container);
   ```

### 2. Configure Google Sheets

1. Create Google Sheet with format:
   ```
   Id,Key,English,Ukrainian,Polish
   1,MainMenu_Play,Play,Грати,Graj
   2,MainMenu_Exit,Exit,Вийти,Wyjdź
   ```

   - **Id** — цілочислове значення enum (унікальне)
   - **Key** — назва ключа в enum

2. Export as CSV (File → Download → Comma-separated values)

3. Place CSV at:
   ```
   Assets/Feature/LocalizationModule/Resources/Localization/Localization.csv
   ```

### 3. Import via Editor

1. Open `Tools → Localization → Load Localization`
2. Paste Google Sheets export URL (or use local CSV)
3. Click "Download and Import"

## Usage

### Static Helper (Recommended)

```csharp
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;

// Get localized string
string text = Loc.Get(LocalizationKey.MainMenu_Play);
```

### Service Injection

```csharp
using Feature.LocalizationModule.Scripts;

public class MyClass
{
    private readonly ILocalizationService _localization;
    
    public MyClass(ILocalizationService localization)
    {
        _localization = localization;
    }
    
    public void UpdateUI()
    {
        string text = _localization.Get(LocalizationKey.MainMenu_Play);
    }
}
```

### UI Component

1. Add `LocalizedText` component to TMP text
2. Select key in Inspector
3. Text updates automatically on language change

### Change Language

```csharp
localizationService.SetLanguage(Language.Ukrainian);
```

## Adding New Languages

### Option 1: Editor (Automatic)

1. Add new column to Google Sheets
2. Re-import via `Tools → Localization → Load Localization`
3. Enum автоматично оновиться

### Option 2: Manual

1. Add value to `Language` enum:
   ```csharp
   public enum Language
   {
       English = 0,
       Ukrainian = 1,
       Polish = 2,
       German = 3  // Add new language
   }
   ```

2. Add column to CSV

## Adding New Keys

### Option 1: Editor (Automatic)

1. Add row to Google Sheets (з новим Id та Key)
2. Re-import via `Tools → Localization → Load Localization`
3. Enum автоматично оновиться

### Option 2: Manual

1. Add value to `LocalizationKey` enum:
   ```csharp
   public enum LocalizationKey
   {
       None = 0,
       MainMenu_Play = 1,
       // Add new key
       NewKey = 14
   }
   ```

2. Add row to CSV

## Editor Tools

| Tool | Path | Description |
|------|------|-------------|
| Load Localization | `Tools → Localization → Load Localization` | Import from Google Sheets + auto-sync enum |
| Validate | `Tools → Localization → Validate Localization` | Check key consistency |

## Performance

- **Lookup**: O(1) via Dictionary
- **Memory**: Pre-cached per language
- **No allocations**: In gameplay code
- **Language switch**: Updates cache, fires event

## Future Extensions

- [ ] Remote config localization
- [ ] Pluralization support
- [ ] String formatting with parameters
- [ ] RTL language support
- [ ] Font fallback per language