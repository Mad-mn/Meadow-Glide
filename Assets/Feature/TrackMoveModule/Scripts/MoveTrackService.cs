using System;
using System.Collections.Generic;
using Feature.CircleModule.Scripts;
using Feature.LoseViewModule.Scripts;
using Feature.SlideAreaModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.TrackMoveModule.Scripts {
    public class MoveTrackService : IMoveTrackService, IInitializable, IDisposable {
        private readonly SlideAreaModel _slideAreaModel;
        private readonly GameCircleModel _gameCircleModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IViewService _viewService;

        private Dictionary<CircleController, float> _circleRotations = new Dictionary<CircleController, float>();
        private Dictionary<CircleSegment, float> _cachedSegmentsRadius = new Dictionary<CircleSegment, float>();

        public MoveTrackService(SlideAreaModel slideAreaModel, GameCircleModel gameCircleModel, MoveTrackModel moveTrackModel,
            IViewService viewService) {
            _slideAreaModel = slideAreaModel;
            _gameCircleModel = gameCircleModel;
            _moveTrackModel = moveTrackModel;
            _viewService = viewService;
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
                _circleRotations[circle] = circle.transform.eulerAngles.z;
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
            if (!_circleRotations.TryGetValue(circle, out float startRotation)) return;

            float currentRotation = circle.transform.eulerAngles.z;
            if (Mathf.Abs(Mathf.DeltaAngle(currentRotation, startRotation)) > 0.1f) {
                _moveTrackModel.Move();
                CheckForLose();
            }
            
            _circleRotations.Remove(circle);
        }

        private void CheckForSpendStepBySlide() {
            if(_cachedSegmentsRadius.Count == 0)return;

            Dictionary<CircleSegment, float> updated = GetSegmentsRadius();
            foreach (KeyValuePair<CircleSegment, float> keyValuePair in _cachedSegmentsRadius) {
                float updatedSegmentRadius = updated[keyValuePair.Key];
                if (!Mathf.Approximately(updatedSegmentRadius, keyValuePair.Value)) {
                    _moveTrackModel.Move();
                    CheckForLose();
                    break;
                }
            }
            
            _cachedSegmentsRadius.Clear();
        }

        private void CheckForLose() {
            if (_moveTrackModel.MovesLeft == 0) {
                _viewService.ShowView<LoseView>(ViewType.LoseView);
            }
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