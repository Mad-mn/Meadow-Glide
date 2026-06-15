using System;
using Feature.ToolModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButton : MonoBehaviour {
        [field: SerializeField] public ToolType ToolType { get; private set; }
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public GameObject LockIcon { get; private set; }
        
        public event Action<ToolType> OnButtonClick;

        private void Awake() {
            Button.onClick.AddListener(ButtonClicked);
        }
        
        private void OnDestroy() {
            Button.onClick.RemoveListener(ButtonClicked);
        }

        private void ButtonClicked() {
            OnButtonClick?.Invoke(ToolType);
        }
    }
}