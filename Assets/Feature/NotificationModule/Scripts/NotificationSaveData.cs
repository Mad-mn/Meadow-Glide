using System;
using Feature.SaveDataModule.Scripts;

namespace Feature.NotificationModule.Scripts {
    [Serializable]
    public class NotificationSaveData : ISaveData {
        public int ScheduledNotificationId;
        public long ScheduledNotificationTimestamp;
        public bool HasUnlockedDailyChallenge;
        public string LastLaunchDate = "";
    }
}
