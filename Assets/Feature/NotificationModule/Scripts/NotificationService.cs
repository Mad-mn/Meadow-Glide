using System;
using Feature.LocalizationModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using UnityEngine;

namespace Feature.NotificationModule.Scripts {
    public class NotificationService : INotificationService {
        private const int DAILY_CHALLENGE_UNLOCK_LEVEL = 12;
        private const int NOTIFICATION_HOUR = 10;

        private readonly INotificationConfigProvider _configProvider;
        private readonly INotificationScheduler _scheduler;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly ILocalizationService _localizationService;

        public NotificationService(
            INotificationConfigProvider configProvider,
            INotificationScheduler scheduler,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            ILocalizationService localizationService) {
            _configProvider = configProvider;
            _scheduler = scheduler;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _localizationService = localizationService;
        }

        public void Initialize() {
            PlayerSettingsData settings = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            if (settings != null && !settings.NotificationsEnabled)
                return;

            NotificationSaveData data = GetOrCreateData();

            if (!data.HasUnlockedDailyChallenge) {
                CheckAndUnlockDailyChallenge(data);
                if (!data.HasUnlockedDailyChallenge)
                    return;
            }

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (data.LastLaunchDate != today) {
                data.LastLaunchDate = today;
                SaveData(data);
            }

            _scheduler.Initialize();
            EnsureTomorrowNotificationScheduled(data);
        }

        public void OnDailyChallengeUnlocked() {
            PlayerSettingsData settings = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            if (settings != null && !settings.NotificationsEnabled)
                return;

            NotificationSaveData data = GetOrCreateData();

            if (!data.HasUnlockedDailyChallenge) {
                data.HasUnlockedDailyChallenge = true;
                SaveData(data);
            }

            _scheduler.Initialize();
            EnsureTomorrowNotificationScheduled(data);
        }

        public void SetNotificationsEnabled(bool enabled) {
            if (enabled) {
                Initialize();
            } else {
                CancelAllNotifications();
            }
        }

        private void CheckAndUnlockDailyChallenge(NotificationSaveData data) {
            PlayerProgressData progress = _saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress);
            if (progress != null && progress.Level >= DAILY_CHALLENGE_UNLOCK_LEVEL) {
                data.HasUnlockedDailyChallenge = true;
                SaveData(data);
                Debug.Log("[Notification] Daily Challenge auto-unlocked");
            }
        }

        private void EnsureTomorrowNotificationScheduled(NotificationSaveData data) {
            NotificationEntry entry = _configProvider.GetEntry(NotificationType.DailyChallenge);
            if (entry == null || !entry.Enabled)
                return;

            DateTime tomorrow10AM = DateTime.Today.AddDays(1).AddHours(NOTIFICATION_HOUR);

            if (data.ScheduledNotificationTimestamp > 0) {
                DateTime scheduledTime = DateTimeOffset.FromUnixTimeSeconds(data.ScheduledNotificationTimestamp).LocalDateTime;

                if (scheduledTime >= tomorrow10AM && scheduledTime < tomorrow10AM.AddDays(1))
                    return;
            }

            ScheduleNotification(entry, tomorrow10AM, data);
        }

        private void ScheduleNotification(NotificationEntry entry, DateTime fireTime, NotificationSaveData data) {
            string title = _localizationService.Get(entry.TitleKey);
            string body = _localizationService.Get(entry.BodyKey);

            int notificationId = GenerateNotificationId();

            _scheduler.Schedule(notificationId, title, body, fireTime);

            data.ScheduledNotificationId = notificationId;
            data.ScheduledNotificationTimestamp = new DateTimeOffset(fireTime).ToUnixTimeSeconds();
            SaveData(data);

            Debug.Log($"[Notification] Scheduled Daily Challenge at {fireTime} (ID: {notificationId})");
        }

        private void CancelAllNotifications() {
            _scheduler.CancelAll();

            NotificationSaveData data = GetOrCreateData();
            data.ScheduledNotificationId = 0;
            data.ScheduledNotificationTimestamp = 0;
            data.LastLaunchDate = "";
            SaveData(data);

            Debug.Log("[Notification] All notifications cancelled");
        }

        private int GenerateNotificationId() {
            return (int)(DateTimeOffset.UtcNow.ToUnixTimeSeconds() % int.MaxValue);
        }

        private NotificationSaveData GetOrCreateData() {
            NotificationSaveData data = _saveDataModel.Get<NotificationSaveData>(SaveDataType.Notifications);
            if (data == null) {
                data = new NotificationSaveData();
            }
            return data;
        }

        private void SaveData(NotificationSaveData data) {
            _saveDataModel.Set(SaveDataType.Notifications, data);
            _saveDataService.Save(SaveDataType.Notifications);
        }
    }
}
