using System;
using System.Collections.Generic;
using Feature.ColorServiceModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.WinLevelModule.Scripts;
using UnityEngine;
using Zenject;

namespace Feature.CircleModule.Scripts {
    public class CircleControllerService : ICircleControllerService, IInitializable, IDisposable {
        private readonly GameCircleModel _circleModel;
        private readonly IViewService _viewService;

        private List<CircleController> _filledCircles = new List<CircleController>();

        public CircleControllerService(GameCircleModel circleModel, IViewService viewService) {
            _circleModel = circleModel;
            _viewService = viewService;
        }

        public void Initialize() {
            _circleModel.OnSegmentsChanged += OnCircleSegmentChanged;
        }

        public void Dispose() {
            _circleModel.OnSegmentsChanged -= OnCircleSegmentChanged;
        }

        public void Reset() {
            _filledCircles.Clear();
        }

        private void OnCircleSegmentChanged() {
            CheckCheckForMatchColors();
        }

        private void CheckCheckForMatchColors() {
            UpdateCirclesStates();
            CheckForWin();
        }

        private void CheckForWin() {
            if (_filledCircles.Count == _circleModel.Circles.Count && _circleModel.Circles.Count > 0) {
                _viewService.ShowView<WinLevel>(ViewType.WinLevel);
            }
        }

        private void UpdateCirclesStates() {
            foreach (CircleController circle in _circleModel.Circles) {
                bool circleFull = IsCircleUniform(circle);
                ApplyResultForCircle(circle, circleFull);
            }
        }

        private bool IsCircleUniform(CircleController circle) {
            if (circle.SpawnedSegments.Count == 0 || circle.SpawnedSegments.Count != circle.SegmentCount) {
                return false;
            }

            CircleColorType targetColor = CircleColorType.None;

            foreach (CircleSegment segment in circle.SpawnedSegments) {
                if (targetColor == CircleColorType.None) {
                    targetColor = segment.ColorType;
                    
                    // None or White usually shouldn't count as a completed circle color
                    if (targetColor == CircleColorType.None) {
                        return false;
                    }
                    continue;
                }

                if (segment.ColorType != targetColor) {
                    return false;
                }
            }

            return true;
        }

        private void ApplyResultForCircle(CircleController circle, bool circleFull) {
            if (circleFull) {
                if(_filledCircles.Contains(circle))
                    return;

                _filledCircles.Add(circle);
            }
            else {
                if(!_filledCircles.Contains(circle))
                    return;

                _filledCircles.Remove(circle);
            }
        }
    }
}