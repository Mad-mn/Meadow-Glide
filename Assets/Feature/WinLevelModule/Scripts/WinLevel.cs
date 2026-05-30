using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevel : ViewBase {
        [field: SerializeField] public Button NextButton{get; private set;}
        [field: SerializeField] public Button MainMenuButton{get; private set;}
    }
}