using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.MessageViewModule.Scripts {
    public class MessageViewModel {
        public event Action<LocalizationKey> OnMessageRequested;
        public event Action HideRequested;

        private LocalizationKey _pendingMessage;
        private bool _hasPending;

        public void SetMessage(LocalizationKey message) {
            _pendingMessage = message;
            _hasPending = true;
            OnMessageRequested?.Invoke(message);
        }

        public void Hide() {
            HideRequested?.Invoke();
        }

        public LocalizationKey ConsumePending() {
            if (!_hasPending)
                return LocalizationKey.None;

            _hasPending = false;
            return _pendingMessage;
        }
    }
}
