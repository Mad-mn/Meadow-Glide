using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.TutorialViewModule.Scripts {
    public class TutorialViewModel {
        public event Action<LocalizationKey> OnTextRequested;

        private LocalizationKey _pendingKey;
        private bool _hasPending;

        public void RequestText(LocalizationKey key) {
            _pendingKey = key;
            _hasPending = true;
            OnTextRequested?.Invoke(key);
        }

        public LocalizationKey ConsumePending() {
            if (!_hasPending)
                return LocalizationKey.None;

            _hasPending = false;
            return _pendingKey;
        }
    }
}
