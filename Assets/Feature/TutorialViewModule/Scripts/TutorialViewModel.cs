using System;
using System.Collections.Generic;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.TutorialViewModule.Scripts {
    public class TutorialViewModel {
        public event Action<LocalizationKey, int> OnTextRequested;

        private LocalizationKey _pendingKey;
        private int _pendingZone;
        private bool _hasPending;
        private IReadOnlyList<int> _textZones;
        private int _currentStep;

        public void SetTextZones(IReadOnlyList<int> textZones) {
            _textZones = textZones;
            _currentStep = 0;
        }

        public void RequestText(LocalizationKey key) {
            int zone = ResolveZone();
            _pendingKey = key;
            _pendingZone = zone;
            _hasPending = true;
            _currentStep++;
            OnTextRequested?.Invoke(key, zone);
        }

        public LocalizationKey ConsumePending(out int zone) {
            if (!_hasPending) {
                zone = 0;
                return LocalizationKey.None;
            }

            _hasPending = false;
            zone = _pendingZone;
            return _pendingKey;
        }

        private int ResolveZone() {
            if (_textZones == null || _currentStep >= _textZones.Count)
                return 0;
            return _textZones[_currentStep];
        }
    }
}
