using System;
using Feature.SaveDataModule.Scripts;

namespace Feature.NotificationModule.Scripts {
    [Serializable]
    public class NotificationSaveData : ISaveData {
        public long DailyChallengeScheduledTimestamp;
        public bool HasScheduledNotification;
        public bool HasUnlockedDailyChallenge;
        public string LastScheduledDate = "";
    }
}
