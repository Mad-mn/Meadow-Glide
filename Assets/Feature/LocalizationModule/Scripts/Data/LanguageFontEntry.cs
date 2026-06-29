using System;
using TMPro;
using UnityEngine;

namespace Feature.LocalizationModule.Scripts.Data {
    [Serializable]
    public class LanguageFontEntry {
        [SerializeField] private Language _language;
        [SerializeField] private TMP_FontAsset _font;

        public Language Language => _language;
        public TMP_FontAsset Font => _font;
    }
}
