using System;
using System.Collections.Generic;
using UnityEngine;

namespace Feature.CircleModule.Scripts {
    public class GameCircleModel {
        private List<CircleController> _circles = new List<CircleController>();
        
        public event Action OnSegmentsChanged;
        
        public IReadOnlyList<CircleController> Circles => _circles;

        public void RegisterCircle(CircleController circle) {
            if(_circles.Contains(circle))
            {
                Debug.LogError("Circle already registered");
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

        public void SegmentsChanged() {
            OnSegmentsChanged?.Invoke();
        }
    }
}