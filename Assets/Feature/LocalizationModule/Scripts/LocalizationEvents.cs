using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.LocalizationModule.Scripts
{
    public static class LocalizationEvents
    {
        public static event Action OnLanguageChanged;

        public static void RaiseLanguageChanged()
        {
            OnLanguageChanged?.Invoke();
        }
    }
}