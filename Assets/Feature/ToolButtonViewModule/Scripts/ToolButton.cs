using System;
using Feature.ToolModule.Scripts;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.ToolButtonViewModule.Scripts {
    public class ToolButton : MonoBehaviour {
        [field: SerializeField] public ToolType ToolType { get; private set; }
        [field: SerializeField] public Button Button { get; private set; }
        [field: SerializeField] public GameObject LockIcon { get; private set; }
        [field: SerializeField] public GameObject AmountContainer { get; private set; }
        [field: SerializeField] public GameObject PriceContainer { get; private set; }
        [field: SerializeField] public TMP_Text AmountText { get; private set; }
        [field: SerializeField] public TMP_Text PriveText { get; private set; }
        
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

        public void SetupView(ToolButtonViewData data) {
            if (data.Blocked) {
                SetupForBlocked();
                return;
            }
            
            LockIcon.SetActive(false);
            if (data.HasTool) {
                PriceContainer.SetActive(false);
                AmountContainer.SetActive(true);
                AmountText.text = data.Amount.ToString();
            } else {
                PriceContainer.SetActive(true);
                AmountContainer.SetActive(false);
                PriveText.text = data.Price.ToString();
            }
        }

        private void SetupForBlocked() {
            LockIcon.SetActive(true);
            AmountContainer.SetActive(false);
            PriceContainer.SetActive(false);
        }
    }

    public struct ToolButtonViewData {
        public bool HasTool;
        public int Amount;
        public int Price;
        public bool Blocked;
    }
}