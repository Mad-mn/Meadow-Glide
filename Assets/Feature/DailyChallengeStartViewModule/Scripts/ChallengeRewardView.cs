using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.DailyChallengeStartViewModule.Scripts {
    public class ChallengeRewardView : MonoBehaviour {
        [SerializeField] private Image _icon;
        [SerializeField] private TMP_Text _amountText;
        [SerializeField] private RectTransform _groupRect;

        public void Setup(Sprite icon, int amount) {
            if (_icon != null)
                _icon.sprite = icon;

            if (_amountText != null)
                _amountText.text = amount.ToString();
            LayoutRebuilder.ForceRebuildLayoutImmediate(_groupRect);
        }
    }
}
