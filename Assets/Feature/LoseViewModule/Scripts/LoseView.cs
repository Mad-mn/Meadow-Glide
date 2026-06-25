using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.LoseViewModule.Scripts {
    public class LoseView : ViewBase {
        [field: SerializeField] public Button RestartButton{get; private set;}
        [field: SerializeField] public Button MainMenuButton{get; private set;}
        [field: SerializeField] public Button AddMovesButton{get; private set;}
        [field: SerializeField] public TMP_Text AddMovesButtonText {get; private set;}
        
    }
}