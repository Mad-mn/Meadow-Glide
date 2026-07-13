using Cysharp.Threading.Tasks;

namespace Feature.NotificationModule.Scripts {
    public class NotificationConfigProvider : INotificationConfigProvider {
        private readonly UniTask<NotificationConfig> _configTask;
        private NotificationConfig _config;

        public NotificationConfigProvider(UniTask<NotificationConfig> configTask) {
            _configTask = configTask;
        }

        public async UniTask Initialize() {
            _config = await _configTask;
        }

        public NotificationEntry GetEntry(NotificationType type) {
            if (_config == null)
                return null;

            return _config.GetEntry(type);
        }
    }
}
