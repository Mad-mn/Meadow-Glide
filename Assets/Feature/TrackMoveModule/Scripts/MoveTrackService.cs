using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackService : IMoveTrackService, IInitializable, IDisposable {
        private readonly SlideAreaModel _slideAreaModel;
        
        private Dictionary<CircleSegment, float> _cachedSegmentsRadius = new Dictionary<CircleSegment, float>();

        public MoveTrackService(SlideAreaModel slideAreaModel) {
            _slideAreaModel = slideAreaModel;
        }

        public void Initialize() {
            _slideAreaModel.OnChangeSlideState += OnChangeSlideState;
        }

        public void Dispose() {
            _slideAreaModel.OnChangeSlideState -= OnChangeSlideState;
        }

        private void OnChangeSlideState(bool slideState) {
            if (slideState) {
                _cachedSegmentsRadius = GetSegmentsRadius();
            }
            else {
                CheckForSpendStepBySlide();
            }
        }

        private void CheckForSpendStepBySlide() {
            if(_cachedSegmentsRadius.Count == 0)return;

            Dictionary<CircleSegment, float> updated = GetSegmentsRadius();
            foreach (KeyValuePair<CircleSegment, float> keyValuePair in _cachedSegmentsRadius) {
                float updatedSegmentRadius = updated[keyValuePair.Key];
                if (!Mathf.Approximately(updatedSegmentRadius, keyValuePair.Value)) {
                    Debug.LogError("SpendStep");
                    break;
                }
            }
            
            _cachedSegmentsRadius.Clear();
        }

        private Dictionary<CircleSegment, float> GetSegmentsRadius() {
            Dictionary<CircleSegment, float> current = new Dictionary<CircleSegment, float>();
            foreach (CircleSegment circleSegment in _slideAreaModel.ActiveSegments) {
                current.Add(circleSegment, circleSegment.Radius);
            }

            return current;
        }
    }
}