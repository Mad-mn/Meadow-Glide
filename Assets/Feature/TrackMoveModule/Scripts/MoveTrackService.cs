using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackService : IMoveTrackService, IInitializable, IDisposable {
        private readonly SlideAreaModel _slideAreaModel;
        private readonly GameCircleModel _gameCircleModel;

        private Dictionary<CircleSegment, float> _cachedSegmentsRadius = new Dictionary<CircleSegment, float>();
        private float _circleRotation;

        public MoveTrackService(SlideAreaModel slideAreaModel, GameCircleModel gameCircleModel) {
            _slideAreaModel = slideAreaModel;
            _gameCircleModel = gameCircleModel;
        }

        public void Initialize() {
            _slideAreaModel.OnChangeSlideState += OnChangeSlideState;
            _gameCircleModel.OnCircleRotationStatusChanged += OnCircleRotationStatusChanged;
        }

        public void Dispose() {
            _slideAreaModel.OnChangeSlideState -= OnChangeSlideState;
            _gameCircleModel.OnCircleRotationStatusChanged -= OnCircleRotationStatusChanged;
        }

        private void OnCircleRotationStatusChanged(CircleController circle, bool isRotating) {
            if (isRotating) {
                _circleRotation = circle.transform.rotation.z % 360;
            }
            else {
                CheckForSpendByRotation(circle);
            }
        }

        private void OnChangeSlideState(bool slideState) {
            if (slideState) {
                _cachedSegmentsRadius = GetSegmentsRadius();
            }
            else {
                CheckForSpendStepBySlide();
            }
        }

        private void CheckForSpendByRotation(CircleController circle) {
            float currentRotation = circle.transform.rotation.z % 360;
            if (!Mathf.Approximately(currentRotation, _circleRotation)) {
                Debug.LogError("MoveSpend");
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