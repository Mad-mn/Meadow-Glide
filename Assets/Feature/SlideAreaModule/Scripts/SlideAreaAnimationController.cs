using System.Collections;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace Feature.SlideAreaModule.Scripts {
    public class SlideAreaAnimationController : MonoBehaviour {
        [SerializeField] private LineRenderer _leftRail;
        [SerializeField] private LineRenderer _rightRail;
        private bool _isAnimating;

        public void PlayBlockedAnimation() {
            if(_isAnimating) return;

            StartCoroutine(TriggerBlockedAnimation());
        }

        private IEnumerator TriggerBlockedAnimation() {
            if (_isAnimating)
                yield break;

            _isAnimating = true;

            float duration = 0.2f;
            float elapsed = 0;
            float startWidth = 1f;
            float targetWidth = 1.3f;

            while (elapsed < duration) {
                elapsed += Time.deltaTime;
                float t = elapsed / duration;
                float curve = Mathf.Sin(t * Mathf.PI); // Pulse
                float width = Mathf.Lerp(startWidth, targetWidth, curve);

                if (_leftRail != null)
                    _leftRail.widthMultiplier = width;

                if (_rightRail != null)
                    _rightRail.widthMultiplier = width;

                yield return null;
            }

            if (_leftRail != null)
                _leftRail.widthMultiplier = startWidth;

            if (_rightRail != null)
                _rightRail.widthMultiplier = startWidth;

            _isAnimating = false;
        }
    }
}