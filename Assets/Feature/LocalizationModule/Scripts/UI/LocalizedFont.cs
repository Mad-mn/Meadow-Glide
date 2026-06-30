using Feature.LocalizationModule.Scripts.Data;
using TMPro;
using UnityEngine;
using Zenject;

namespace Feature.LocalizationModule.Scripts.UI {
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedFont : MonoBehaviour {
        [SerializeField] private LanguageFontConfig _config;
        private TMP_Text _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable() {
            LocalizationEvents.OnLanguageChanged += OnLanguageChanged;
            ApplyFont();
        }

        private void OnDisable() {
            LocalizationEvents.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() {
            ApplyFont();
        }

        private void ApplyFont() {
            if (_textComponent == null)
                _textComponent = GetComponent<TMP_Text>();

            TMP_FontAsset font = _config.GetFont(Loc.CurrentLanguage());
            if (font != null) {
                _textComponent.font = font;
            }
        }
    }
}
