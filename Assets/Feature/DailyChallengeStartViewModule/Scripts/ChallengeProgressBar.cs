using UnityEngine;
using UnityEngine.UI;

namespace Feature.DailyChallengeStartViewModule.Scripts {
    public class ChallengeProgressBar : MonoBehaviour {
        [SerializeField] private Image _fill;

        public void SetFill(float normalized) {
            if (_fill != null)
                _fill.fillAmount = Mathf.Clamp01(normalized);
        }
    }
}
