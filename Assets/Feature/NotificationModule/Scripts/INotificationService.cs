namespace Feature.NotificationModule.Scripts {
    public interface INotificationService {
        void Initialize();
        void OnDailyChallengeUnlocked();
        void SetNotificationsEnabled(bool enabled);
    }
}
