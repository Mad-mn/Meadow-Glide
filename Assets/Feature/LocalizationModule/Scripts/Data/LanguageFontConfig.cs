using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts.Data {
    [CreateAssetMenu(fileName = "LanguageFontConfig", menuName = "Configs/Localization/LanguageFontConfig")]
    public class LanguageFontConfig : ScriptableObject {
        [SerializeField] private List<LanguageFontEntry> _entries = new List<LanguageFontEntry>();
        [SerializeField] private TMP_FontAsset _defaultFont;

        public IReadOnlyList<LanguageFontEntry> Entries => _entries;
        public TMP_FontAsset DefaultFont => _defaultFont;

        public TMP_FontAsset GetFont(Language language) {
            foreach (var entry in _entries) {
                if (entry.Language == language)
                    return entry.Font;
            }
            return _defaultFont;
        }
    }
}
