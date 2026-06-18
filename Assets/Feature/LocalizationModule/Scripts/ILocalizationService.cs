using Feature.LocalizationModule.Scripts.Data;

namespace Feature.LocalizationModule.Scripts
{
    public interface ILocalizationService {
        void Initialize();
        Language CurrentLanguage { get; }
        void SetLanguage(Language language);
        string Get(LocalizationKey key);
        string Get(string key);
        bool HasKey(LocalizationKey key);
        bool HasKey(string key);
        void Reload();
    }
}