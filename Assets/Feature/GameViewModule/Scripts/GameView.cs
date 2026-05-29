using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.GameViewModule.Scripts {
    public class GameView : ViewBase {
        [SerializeField] private Button _mainMenuButton;
        
        public Button MainMenuButton => _mainMenuButton;
    }
}