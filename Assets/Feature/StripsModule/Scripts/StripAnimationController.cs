using System;
using System.Collections;
using Feature.CircleModule.Scripts;
using UnityEngine;
using UnityEngine.Serialization;

namespace Feature.StripsModule.Scripts {
    public class StripAnimationController : MonoBehaviour{
        [SerializeField] private StripController _strip;
        [SerializeField] private float _segmentZoomDuration;

        private bool _completedAnimationPlaying;
        
        public void PlayCompletedAnimation(Action callback) {
            if(_completedAnimationPlaying)
                return;
            
            _completedAnimationPlaying = true;
            StartCoroutine(CompletedAnimation(callback));
        }

        private IEnumerator CompletedAnimation(Action callback) {
            WaitForSeconds wait = new WaitForSeconds(_segmentZoomDuration);
            foreach (CircleSegment segment in _strip.SpawnedSegments) {
                segment.ZoomIn();
                yield return wait;
                segment.ZoomOut();
            }

            yield return wait;
            callback?.Invoke();
            _completedAnimationPlaying = false;
        }
    }
}