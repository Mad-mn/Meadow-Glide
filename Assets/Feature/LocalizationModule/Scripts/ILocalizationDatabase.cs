using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.LocalizationModule.Scripts
{
    public interface ILocalizationDatabase
    {
        IReadOnlyDictionary<Language, Dictionary<LocalizationKey, string>> GetAllData();
        Dictionary<LocalizationKey, string> GetLanguageData(Language language);
        void LoadFromCsv(string csvContent);
        void LoadFromJson(string jsonContent);
        string ExportToJson();
    }
}