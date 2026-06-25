using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.SettingsViewModule.Scripts {
    public class SettingsView : ViewBase {
        [field: SerializeField] public Button CloseButton { get; private set; }
        [field: SerializeField] public Toggle SoundsToggle { get; private set; }
        [field: SerializeField] public Toggle VibrationToggle { get; private set; }
        [field: SerializeField] public TMP_Dropdown Language { get; private set; }
    }
}