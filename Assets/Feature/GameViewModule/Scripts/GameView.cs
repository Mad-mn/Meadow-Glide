using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.GameViewModule.Scripts {
    public class GameView : ViewBase {
        [SerializeField] private Button _mainMenuButton;
        [field: SerializeField] public Button ResetLevelButton { get; private set; }
        [SerializeField] private TMP_Text _levelTxt;
        [SerializeField] private TMP_Text _moveCountTxt;
        
        public Button MainMenuButton => _mainMenuButton;
        public void SetLevelText(string text) => _levelTxt.text = text;
        public void SetMoveCount(string text) => _moveCountTxt.text = text;
    }
}