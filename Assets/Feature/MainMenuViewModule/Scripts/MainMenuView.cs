using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuView : ViewBase {
        [SerializeField] private Button _playButton;
        
        public Button PlayButton => _playButton;
    }
}