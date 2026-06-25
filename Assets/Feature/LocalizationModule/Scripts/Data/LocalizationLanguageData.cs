using System.Collections.Generic;

namespace Feature.LocalizationModule.Scripts.Data
{
    [System.Serializable]
    public class LocalizationLanguageData
    {
        public Language Language;
        public List<LocalizationEntry> Entries = new List<LocalizationEntry>();
    }
}