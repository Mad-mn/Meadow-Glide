using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;
using Feature.LocalizationModule.Scripts.Utils;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts
{
    [System.Serializable]
    public class LocalizationDatabase : ILocalizationDatabase
    {
        private const string CSV_RESOURCE_PATH = "Localization/Localization";

        private readonly Dictionary<Language, Dictionary<LocalizationKey, string>> _data =
            new Dictionary<Language, Dictionary<LocalizationKey, string>>();

        private bool _isLoaded;

        public void LoadFromResources()
        {
            var csvAsset = Resources.Load<TextAsset>(CSV_RESOURCE_PATH);
            if (csvAsset != null)
            {
                LoadFromCsv(csvAsset.text);
            }
            else
            {
                Debug.LogWarning("[Localization] CSV file not found in Resources. Please import localization data.");
            }
        }

        public IReadOnlyDictionary<Language, Dictionary<LocalizationKey, string>> GetAllData()
        {
            return _data;
        }

        public Dictionary<LocalizationKey, string> GetLanguageData(Language language)
        {
            if (_data.TryGetValue(language, out var langData))
                return langData;

            if (_data.TryGetValue(Language.English, out var fallback))
                return fallback;

            return new Dictionary<LocalizationKey, string>();
        }

        public void LoadFromCsv(string csvContent)
        {
            _data.Clear();
            var (languages, parsedData) = CsvParser.Parse(csvContent);

            foreach (var lang in languages)
            {
                if (parsedData.TryGetValue(lang, out var langData))
                {
                    _data[lang] = langData;
                }
            }

            _isLoaded = true;
            Debug.Log($"[Localization] Loaded {languages.Count} languages from CSV");
        }

        public void LoadFromJson(string jsonContent)
        {
            _data.Clear();

            var wrapper = JsonUtility.FromJson<LocalizationJsonWrapper>(jsonContent);
            if (wrapper?.languages == null) return;

            foreach (var langData in wrapper.languages)
            {
                var langDict = new Dictionary<LocalizationKey, string>();
                foreach (var entry in langData.Entries)
                {
                    langDict[entry.Key] = entry.Value;
                }
                _data[langData.Language] = langDict;
            }

            _isLoaded = true;
            Debug.Log($"[Localization] Loaded {_data.Count} languages from JSON");
        }

        public string ExportToJson()
        {
            var wrapper = new LocalizationJsonWrapper { languages = new List<LocalizationLanguageData>() };

            foreach (var kvp in _data)
            {
                var langData = new LocalizationLanguageData { Language = kvp.Key };
                foreach (var entry in kvp.Value)
                {
                    langData.Entries.Add(new LocalizationEntry(entry.Key, entry.Value));
                }
                wrapper.languages.Add(langData);
            }

            return JsonUtility.ToJson(wrapper, true);
        }

        public void SetLanguageData(Language language, Dictionary<LocalizationKey, string> entries)
        {
            _data[language] = entries;
            _isLoaded = true;
        }
    }

    [System.Serializable]
    public class LocalizationJsonWrapper
    {
        public List<LocalizationLanguageData> languages;
    }
}