using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.LocalizationModule.Scripts
{
    public static class LocalizationEvents
    {
        public static event Action<Language> OnLanguageChanged;

        public static void RaiseLanguageChanged(Language newLanguage)
        {
            OnLanguageChanged?.Invoke(newLanguage);
        }
    }
}