using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.ConfirmExitToMainMenuViewModule.Scripts {
    public class ConfirmExitToMainMenuView : ViewBase {
        [SerializeField] private Button _yesButton;
        [SerializeField] private Button _noButton;
        
        public Button YesButton => _yesButton;
        public Button NoButton => _noButton;
    }
}