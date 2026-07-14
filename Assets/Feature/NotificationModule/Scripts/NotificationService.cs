using System;
using Feature.ChallengeModule.Scripts;
using Feature.LocalizationModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using UnityEngine;

namespace Feature.NotificationModule.Scripts {
    public class NotificationService : INotificationService {
        private readonly INotificationConfigProvider _configProvider;
        private readonly INotificationScheduler _scheduler;
        private readonly IChallengeService _challengeService;
        private readonly ISaveDataModel _saveDataModel;
        private readonly ISaveDataService _saveDataService;
        private readonly ILocalizationService _localizationService;

        public NotificationService(
            INotificationConfigProvider configProvider,
            INotificationScheduler scheduler,
            IChallengeService challengeService,
            ISaveDataModel saveDataModel,
            ISaveDataService saveDataService,
            ILocalizationService localizationService) {
            _configProvider = configProvider;
            _scheduler = scheduler;
            _challengeService = challengeService;
            _saveDataModel = saveDataModel;
            _saveDataService = saveDataService;
            _localizationService = localizationService;
        }

        public void Initialize() {
            PlayerSettingsData settings = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            if (settings != null && !settings.NotificationsEnabled)
                return;

            NotificationSaveData data = GetOrCreateData();
            if (!data.HasUnlockedDailyChallenge)
                return;

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (data.LastScheduledDate == today)
                return;

            if (data.HasScheduledNotification) {
                DateTime scheduledTime = DateTimeOffset.FromUnixTimeSeconds(data.DailyChallengeScheduledTimestamp).LocalDateTime;
                if (scheduledTime > DateTime.Now)
                    return;
            }

            _scheduler.Initialize();
            ScheduleDailyChallengeForTomorrow();
        }

        public void OnDailyChallengeUnlocked() {
            PlayerSettingsData settings = _saveDataModel.Get<PlayerSettingsData>(SaveDataType.Settings);
            if (settings != null && !settings.NotificationsEnabled)
                return;

            NotificationSaveData data = GetOrCreateData();
            if (data.HasUnlockedDailyChallenge)
                return;

            data.HasUnlockedDailyChallenge = true;
            SaveData(data);

            string today = DateTime.Today.ToString("yyyy-MM-dd");
            if (data.LastScheduledDate == today)
                return;

            ScheduleDailyChallengeForTomorrow();
        }

        public void SetNotificationsEnabled(bool enabled) {
            if (enabled) {
                Initialize();
            } else {
                CancelAllNotifications();
            }
        }

        private void CancelAllNotifications() {
            _scheduler.CancelAll();

            NotificationSaveData data = GetOrCreateData();
            data.HasScheduledNotification = false;
            data.DailyChallengeScheduledTimestamp = 0;
            data.LastScheduledDate = "";
            SaveData(data);

            Debug.Log("[Notification] All notifications cancelled");
        }

        private void ScheduleDailyChallengeForTomorrow() {
            NotificationEntry entry = _configProvider.GetEntry(NotificationType.DailyChallenge);
            if (entry == null || !entry.Enabled)
                return;

            DateTime baseTime = DateTime.Now.AddHours(entry.DelayHours);
            DateTime validTime = ApplyDeliveryWindow(baseTime, entry.DeliveryWindowStartHour, entry.DeliveryWindowEndHour);

            string title = _localizationService.Get(entry.TitleKey);
            string body = _localizationService.Get(entry.BodyKey);
            _scheduler.Schedule(entry.NotificationId, title, body, validTime);

            NotificationSaveData data = GetOrCreateData();
            data.DailyChallengeScheduledTimestamp = new DateTimeOffset(validTime).ToUnixTimeSeconds();
            data.HasScheduledNotification = true;
            data.LastScheduledDate = DateTime.Today.ToString("yyyy-MM-dd");
            SaveData(data);

            Debug.Log($"[Notification] Scheduled Daily Challenge at {validTime}");
        }

        private static DateTime ApplyDeliveryWindow(DateTime baseTime, int startHour, int endHour) {
            if (baseTime.Hour >= startHour && baseTime.Hour < endHour) {
                return baseTime;
            }

            if (baseTime.Hour < startHour) {
                return baseTime.Date.AddHours(startHour);
            }

            return baseTime.Date.AddDays(1).AddHours(startHour);
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
