using Feature.UIServiceModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.DebugViewModule.Scripts {
    public class DebugView : ViewBase {
        [field:SerializeField] public Button GoToLevelButton { get; private set; }
        [field:SerializeField] public TMP_InputField GoToLevelInputField { get; private set; }
        [field:SerializeField] public Button CloseDebugButton { get; private set; }
        [field:SerializeField] public Button Add100CoinsBUtton { get; private set; }
    }
}