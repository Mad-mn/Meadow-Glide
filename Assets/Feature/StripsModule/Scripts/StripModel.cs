using System;
using System.Collections.Generic;

namespace Feature.StripsModule.Scripts {
    public class StripModel {
        
        private List<StripController> _strips = new List<StripController>();
        private List<StripController> _completedStrips = new List<StripController>();
        public event Action OnSegmentsChanged;
        public event Action<StripController, bool> OnStripRotationStatusChanged;
        public event Action<StripController, bool> OnStripCompletedStatusChanged;
        
        public IReadOnlyList<StripController> Strips => _strips;

        public void Clear() {
            _strips.Clear();
        }

        public void CircleRotationStatusChanges(StripController circle, bool isRotating) {
            OnStripRotationStatusChanged?.Invoke(circle, isRotating);
        }

        public void SegmentsChanged() {
            OnSegmentsChanged?.Invoke();
        }

        public void ChangeCircleCompleterState(StripController circle, bool isCompleting) {
            if (isCompleting) {
                if(_completedStrips.Contains(circle))
                    return;
                _completedStrips.Add(circle);
                OnStripCompletedStatusChanged?.Invoke(circle, true);
            }
            else {
                if(!_strips.Contains(circle))
                    return;
                _completedStrips.Remove(circle);
                OnStripCompletedStatusChanged?.Invoke(circle, false);
            }
        }
        public void RegisterStrip(StripController strip) {
            if(_strips.Contains(strip))
                return;
            _strips.Add(strip);
        }

        public void UnregisterStrips(StripController strip) {
            if(!_strips.Contains(strip))
                return;
            _strips.Remove(strip);
        }
    }
}