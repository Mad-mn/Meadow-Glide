# Daily Challenge System — Архітектура та Використання

---

## 1. Що це таке

Daily Challenge — це щоденний ігровий виклик, де гравець проходить один складний рівень з пулу. Кожного дня рівень змінюється автоматично. Система мотивує гравця повертатися щодня та перепроходити рівень заради кращої оцінки.

**Ключові характеристики:**
- Один рівень на день для всіх гравців (seed-based вибір)
- Система зірок (1-3) за ефективність використання ходів
- Кумулятивні нагороди з різницею при перепроходженні
- Розблоковується на Level 12
- Не впливає на звичайну прогресію

---

## 2. Архітектура

### 2.1 Модулі

```
StarModule (новий)          — розрахунок зірок
├── StarRating              — enum: None, One, Two, Three
├── IStarCalculator         — інтерфейс калькулятора
└── MoveEfficiencyStarCalculator — формула за efficiency ratio

ChallengeModule (новий)     — логіка челенджів
├── ChallengeType           — enum: Daily, Weekly, Event, Seasonal
├── ChallengeConfig         — ScriptableObject: конфігурація
├── ChallengeConfigs        — колекція конфігів
├── ChallengeConfigProvider — завантаження з Addressables
├── ChallengeLevelSelector  — вибір рівня за датою
├── ChallengeService        — основний сервіс
└── ChallengeSessionData    — дані поточної сесії

SaveDataModule (оновлено)   — збереження стану
└── DailyChallengeData      — LastCompletedDate, TodayStarsEarned, ClaimedStarsThreshold
```

### 2.2 Залежності

```
ChallengeService
├── IChallengeConfigProvider   — отримання конфігурацій
├── ISaveDataModel             — збереження стану
├── ISaveDataService           — персистенція на диск
├── IPlayerInventoryService    — нарахування нагород
└── IStarCalculator            — розрахунок зірок

LevelService (оновлено)
└── IChallengeService          — визначення джерела рівня

CircleControllerService (оновлено)
└── IChallengeService          — обробка завершення челенджу
```

---

## 3. Формула зірок

Розрахунок базується на ефективності використання ходів:

```
movesLeft = maxMoves - movesUsed
ratio = movesLeft / maxMoves

3⭐: ratio >= 0.50  (використав менше половини ходів)
2⭐: ratio >= 0.25  (використав менше 3/4 ходів)
1⭐: ratio >= 0     (просто пройшов рівень)
```

**Приклад:**
- Рівень має 10 ходів
- Гравець виграв за 3 ходи → movesLeft = 7 → ratio = 0.7 → 3⭐
- Гравець виграв за 6 ходів → movesLeft = 4 → ratio = 0.4 → 2⭐
- Гравець виграв за 9 ходів → movesLeft = 1 → ratio = 0.1 → 1⭐

---

## 4. Вибір рівня

Вибір базується на `DateTime.Today.DayOfYear`:

```csharp
int index = DateTime.Today.DayOfYear % levelPool.Count;
LevelConfig levelConfig = levelPool[index];
```

**Властивості:**
- Стабільний протягом дня (один і той самий рівень)
- Різний для різних днів
- Автоматично змінюється при зміні дня
- Одинаковий для всіх гравців того самого дня
- Передбачуваний для тестування

---

## 5. Система нагород

Нагороди кумулятивні з різницею. Гравець отримує нагороду за кожну нову зірку.

**Приклад нагород (налаштовується через ChallengeConfig):**

| Зірки | Монети | Undo | Загалом |
|-------|--------|------|---------|
| 1⭐ | 50 | — | 50 🪙 |
| 2⭐ | 100 | +1 | 150 🪙 + 1 Undo |
| 3⭐ | 200 | +2 | 350 🪙 + 3 Undo |

**Flow:**
```
Спроба 1: 1⭐ → ClaimReward() → отримує 50 Coins
Спроба 2: 2⭐ → ClaimReward() → отримує +100 Coins + 1 Undo (різниця)
Спроба 3: 3⭐ → ClaimReward() → отримує +200 Coins + 2 Undo (різниця)
→ CanPlayToday() = false (досягнуто максимуму)
```

**Правила:**
- Після отримання 3⭐ гравець не може грати знову сьогодні
- Нагорода за вже отримані зірки не видається повторно
- При перепроходженні (1⭐ → 3⭐) гравець отримує тільки різницю

---

## 6. Збереження даних

### 6.1 DailyChallengeData

```csharp
[Serializable]
public class DailyChallengeData : ISaveData {
    public string LastCompletedDate;      // "yyyy-MM-dd"
    public int TodayStarsEarned;          // Максимальні зірки за сьогодні
    public int ClaimedStarsThreshold;     // До якої зірки вже отримав нагороду
}
```

### 6.2 Логіка скидання

При кожному зверненні до `DailyChallengeService` перевіряється дата:

```csharp
string today = DateTime.Today.ToString("yyyy-MM-dd");
if (data.LastCompletedDate != today) {
    data.LastCompletedDate = today;
    data.TodayStarsEarned = 0;
    data.ClaimedStarsThreshold = 0;
}
```

Якщо дата змінилася — всі дані скидаються для нового дня.

### 6.3 Персистенція

- `SaveDataModel` — кеш в пам'яті
- `SaveDataService.Save(DailyChallenge)` — запис на диск через BinaryFormatter
- Файл: `persistentDataPath/dailychallenge_save.dat`

---

## 7. Інтеграція з існуючими системами

### 7.1 LevelService

`GetLevelDataForCurrentLevel()` тепер перевіряє стан челенджу:

```csharp
public LevelData GetLevelDataForCurrentLevel() {
    if (_challengeService.IsActive) {
        return _challengeService.GetCurrentLevel();
    }
    
    PlayerProgressData playerProgressData = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
    return _levelConfigProvider.LevelDatas[playerProgressData.Level];
}
```

### 7.2 CircleControllerService

`ApplyWin()` тепер обробляє челендж окремо:

```csharp
if (_challengeService.IsActive) {
    int movesUsed = _moveTrackModel.MaxMovesForCurrentLevel - _moveTrackModel.MovesLeft;
    _challengeService.OnChallengeCompleted(_moveTrackModel.MaxMovesForCurrentLevel, movesUsed);
}
else {
    _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level++;
    _saveDataService.Save(SaveDataType.PlayerProgress);
    _inventoryService.Add(ResourceType.Coins, _economyDataProvider.EconomyConfig.LevelWinReward);
}
```

**Ключова відмінність:** При челенджі НЕ збільшується номер рівня та НЕ видається базова нагорода.

---

## 8. Налаштування в Unity Editor

### 8.1 Створення конфігурації

1. `Assets > Create > Configs > Challenge > ChallengeConfig`
2. Налаштувати параметри:
   - **Challenge Type** — Daily (або Weekly/Event/Seasonal)
   - **Unlock Level** — 12 (рівень розблокування)
   - **Level Pool** — список LevelConfig об'єктів
   - **Star Rewards** — нагороди за кожну зірку

### 8.2 Створення колекції

1. `Assets > Create > Configs > Challenge > ChallengeConfigs`
2. Додати створений ChallengeConfig до списку

### 8.3 Реєстрація в Addressables

1. Додати ChallengeConfigs до Addressables Group
2. Встановити адресу: `"DailyChallengeConfigs"`
3. Запустити `Tools > GenerateAdresablesNames`

---

## 9. API для використання

### 9.1 Перевірка доступності

```csharp
// Чи доступний daily challenge?
bool available = challengeService.IsDailyChallengeAvailable(currentLevel);

// Чи можна грати сьогодні? (не досягнуто 3⭐)
bool canPlay = challengeService.CanPlayToday();

// Чи є що отримати?
bool canClaim = challengeService.CanClaimReward();
```

### 9.2 Активація челенджу

```csharp
// Активувати daily challenge перед запуском рівня
challengeService.ActivateDailyChallenge(levelConfig);

// Після цього LevelService автоматично поверне рівень з челенджу
```

### 9.3 Завершення рівня

```csharp
// Автоматично викликається в CircleControllerService.ApplyWin()
challengeService.OnChallengeCompleted(maxMoves, movesUsed);
```

### 9.4 Отримання нагороди

```csharp
// Отримати нагороду за поточні зірки
List<ResourceAmount> rewards = challengeService.ClaimReward();

// Показати нагороду гравцю
foreach (ResourceAmount reward in rewards) {
    Debug.Log($"Отримано: {reward.Amount} {reward.Type}");
}
```

### 9.5 Деактивація

```csharp
// При поверненні в MainMenu
challengeService.Deactivate();
```

---

## 10. Масштабованість

### 10.1 Нові типи челенджів

Додавання нового типу (наприклад, Weekly Challenge):

1. Додати значення в `ChallengeType` enum
2. Створити клас який реалізує `IChallengeMode` (опціонально)
3. Налаштувати `ChallengeConfig` з новим типом
4. Додати логіку вибору рівня (наприклад, за тижнем)

### 10.2 Нові типи нагород

Додавання нового ресурсу:

1. Додати значення в `ResourceType` enum
2. Додати в `ChallengeConfig.StarRewards`
3. `IPlayerInventoryService.Add()` вже підтримує будь-який `ResourceType`

### 10.3 Нові формули зірок

Створення альтернативного калькулятора:

```csharp
public class TimeBasedStarCalculator : IStarCalculator {
    public StarRating Calculate(int maxMoves, int movesUsed) {
        // Інша формула
    }
}

// Зареєструвати в StarModuleInstaller
Container.Bind<IStarCalculator>().To<TimeBasedStarCalculator>().AsSingle();
```

---

## 11. Архітектурні рішення

### 11.1 Чому LevelService перевіряє ChallengeService

Замість override патерну, `LevelService` внутрішньо визначає джерело рівня. Це дозволяє:
- Не змінювати інтерфейс `ILevelService`
- Зберегти сумісність з існуючим кодом
- Просто додати нову гілку логіки

### 11.2 Чому нагороди кумулятивні з різницею

Це стандарт для puzzle games:
- Гравець мотивований перепроходити заради кращих зірок
- Нагорода за вже отримані зірки не втрачається
- Створює цикл: грати → отримати зірки → отримати нагороду → хоче більше

### 11.3 Чому DayOfYear % poolCount

Цей підхід найпростіший і найстабільніший:
- Не потребує сервера або зовнішнього стану
- Одинаковий для всіх гравців того самого дня
- Автоматично змінюється при зміні дня
- Масштабується на Weekly: `(DayOfYear / 7) % poolCount`

---

## 12. Обмеження та відомі проблеми

1. **BinaryFormatter** — deprecated, не працює на IL2CPP/WebGL (загальна проблема проекту)
2. **Часові появи** — `DateTime.Today` використовує місцевий час пристрою
3. **Пул рівнів** — обмежений кількістю LevelConfig в конфігурації
4. **Немає онлайн-синхронізації** — стан зберігається локально

---

*Документ створено: 2026-06-18*
*Пов'язані документи: GDDevelopmentPlan.md, CurrencyBalanceDocument.md*
