using Feature.LocalizationModule.Scripts.Data;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts
{
    public static class Loc
    {
        private static ILocalizationService _service;

        public static void Initialize(ILocalizationService service)
        {
            _service = service;
        }

        public static string Get(LocalizationKey key)
        {
            if (_service == null)
            {
                Debug.LogWarning("[Localization] Service not initialized. Call Loc.Initialize() first.");
                return key.ToString();
            }

            return _service.Get(key);
        }

        public static string Get(string key)
        {
            if (_service == null)
            {
                Debug.LogWarning("[Localization] Service not initialized. Call Loc.Initialize() first.");
                return key;
            }

            return _service.Get(key);
        }
        
        public static Language CurrentLanguage()
        {
            if (_service == null)
            {
                Debug.LogWarning("[Localization] Service not initialized. Call Loc.Initialize() first.");
                return Language.English;
            }

            return _service.CurrentLanguage;
        }
    }
}