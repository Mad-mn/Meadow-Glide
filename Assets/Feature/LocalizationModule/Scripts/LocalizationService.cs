using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts
{
    public class LocalizationService : ILocalizationService
    {
        private readonly ILocalizationDatabase _database;
        private readonly ISaveDataService _saveDataService;
        private readonly SaveDataModel _saveDataModel;

        private Language _currentLanguage = Language.English;
        private Dictionary<LocalizationKey, string> _currentLanguageCache;
        private bool _isInitialized;

        public Language CurrentLanguage => _currentLanguage;

        public LocalizationService(ILocalizationDatabase database, ISaveDataService saveDataService, SaveDataModel saveDataModel)
        {
            _database = database;
            _saveDataService = saveDataService;
            _saveDataModel = saveDataModel;
        }

        public void Initialize()
        {
            if (_database is LocalizationDatabase db)
            {
                db.LoadFromResources();
            }

            LoadSavedLanguage();
            UpdateCache();
            _isInitialized = true;
            Loc.Initialize(this);
        }

        public void SetLanguage(Language language)
        {
            if (_currentLanguage == language) return;

            _currentLanguage = language;
            UpdateCache();
            SaveLanguage();
            LocalizationEvents.RaiseLanguageChanged(language);
        }

        public string Get(LocalizationKey key)
        {
            if (!_isInitialized)
            {
                Debug.LogWarning("[Localization] Service not initialized. Call Initialize() first.");
                return key.ToString();
            }

            if (_currentLanguageCache != null && _currentLanguageCache.TryGetValue(key, out var value))
                return value;

            if (TryGetFallback(key, out var fallback))
                return fallback;

            Debug.LogWarning($"[Localization] Key not found: {key}");
            return key.ToString();
        }

        public string Get(string key)
        {
            if (System.Enum.TryParse<LocalizationKey>(key, true, out var enumKey))
                return Get(enumKey);

            Debug.LogWarning($"[Localization] Invalid key string: {key}");
            return key;
        }

        public bool HasKey(LocalizationKey key)
        {
            return _currentLanguageCache != null && _currentLanguageCache.ContainsKey(key);
        }

        public bool HasKey(string key)
        {
            if (System.Enum.TryParse<LocalizationKey>(key, true, out var enumKey))
                return HasKey(enumKey);

            return false;
        }

        public void Reload()
        {
            UpdateCache();
        }

        private void UpdateCache()
        {
            _currentLanguageCache = _database.GetLanguageData(_currentLanguage);
        }

        private bool TryGetFallback(LocalizationKey key, out string value)
        {
            if (_database.GetAllData().TryGetValue(Language.English, out var enData) &&
                enData.TryGetValue(key, out value))
            {
                return true;
            }

            foreach (var langData in _database.GetAllData().Values)
            {
                if (langData.TryGetValue(key, out value))
                    return true;
            }

            value = null;
            return false;
        }

        private void LoadSavedLanguage()
        {
            var settings = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            _currentLanguage = settings.SelectedLanguage;
        }

        private void SaveLanguage()
        {
            _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings).SelectedLanguage = _currentLanguage;
            _saveDataService.Save(SaveDataType.Settings);
        }
    }
}