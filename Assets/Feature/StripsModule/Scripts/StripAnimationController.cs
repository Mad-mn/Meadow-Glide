using System;
using System.Collections;
using Feature.StripsModule.Scripts;
using UnityEngine;

namespace Feature.StripsModule.Scripts {
    public class StripAnimationController : MonoBehaviour {
        [SerializeField] private StripController _strip;
        [SerializeField] private float _segmentZoomDuration;
        [SerializeField] private float _secondsDelayAfterCompleted = 2f;

        private bool _completedAnimationPlaying;

        public void PlayCompletedAnimation(Action callback) {
            if (_completedAnimationPlaying)
                return;

            _completedAnimationPlaying = true;
            StartCoroutine(CompletedAnimation(callback));
        }

        private IEnumerator CompletedAnimation(Action callback) {
            WaitForSeconds wait = new WaitForSeconds(_segmentZoomDuration);
            foreach (StripSegment segment in _strip.SpawnedSegments) {
                segment.ZoomIn();
                yield return wait;
                segment.ZoomOut();
            }

            yield return new WaitForSeconds(_secondsDelayAfterCompleted);
            callback?.Invoke();
            _completedAnimationPlaying = false;
        }
    }
}
