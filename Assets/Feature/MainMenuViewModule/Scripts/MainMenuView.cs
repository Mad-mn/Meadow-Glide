using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuView : ViewBase {
        [SerializeField] private Button _playButton;
        [SerializeField] private TMP_Text _levelText;
        [field: SerializeField] public Button DebugButton { get; private set; }
        
        public Button PlayButton => _playButton;
        public void LevelText(string text) => _levelText.text = text;
    }
}