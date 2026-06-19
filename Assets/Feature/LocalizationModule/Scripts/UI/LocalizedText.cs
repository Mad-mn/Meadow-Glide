using Feature.LocalizationModule.Scripts.Data;
using TMPro;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts.UI {
    [RequireComponent(typeof(TMP_Text))]
    public class LocalizedText : MonoBehaviour {
        [SerializeField] private LocalizationKey _key;
        [SerializeField] private bool _updateOnLanguageChange = true;

        private TMP_Text _textComponent;

        private void Awake() {
            _textComponent = GetComponent<TMP_Text>();
        }

        private void OnEnable() {
            if (_updateOnLanguageChange) {
                LocalizationEvents.OnLanguageChanged += OnLanguageChanged;
            }

            UpdateText();
        }

        private void OnDisable() {
            LocalizationEvents.OnLanguageChanged -= OnLanguageChanged;
        }

        private void OnLanguageChanged() {
            UpdateText();
        }

        public void UpdateText() {
            if (_textComponent == null)
                _textComponent = GetComponent<TMP_Text>();

            if (_key == LocalizationKey.None)
                return;

            _textComponent.text = Loc.Get(_key);
        }
    }
}