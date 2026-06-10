using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.CircleModule.Scripts {
    public class GameCircleModel {
        private List<CircleController> _circles = new List<CircleController>();
        private List<CircleController> _completedCircles = new List<CircleController>();
        public event Action OnSegmentsChanged;
        public event Action<CircleController, bool> OnCircleRotationStatusChanged;
        public event Action<CircleController, bool> OnCircleCompletedStatusChanged;
        
        public IReadOnlyList<CircleController> Circles => _circles;

        public void RegisterCircle(CircleController circle) {
            if(_circles.Contains(circle))
            {
                return;
            }

            _circles.Add(circle);
        }

        public void UnregisterCircle(CircleController circle) {
            if(_circles.Contains(circle))
                _circles.Remove(circle);
        }

        public void Clear() {
            _circles.Clear();
        }

        public void CircleRotationStatusChanges(CircleController circle, bool isRotating) {
            OnCircleRotationStatusChanged?.Invoke(circle, isRotating);
        }

        public void SegmentsChanged() {
            OnSegmentsChanged?.Invoke();
        }

        public void ChangeCircleCompleterState(CircleController circle, bool isCompleting) {
            if (isCompleting) {
                if(_completedCircles.Contains(circle))
                    return;
                _completedCircles.Add(circle);
                OnCircleCompletedStatusChanged?.Invoke(circle, true);
            }
            else {
                if(!_circles.Contains(circle))
                    return;
                _completedCircles.Remove(circle);
                OnCircleCompletedStatusChanged?.Invoke(circle, false);
            }
        }
    }
}