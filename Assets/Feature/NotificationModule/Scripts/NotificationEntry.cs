using System;
using Feature.LocalizationModule.Scripts.Data;

namespace Feature.NotificationModule.Scripts {
    [Serializable]
    public class NotificationEntry {
        public NotificationType Type;
        public int NotificationId;
        public LocalizationKey TitleKey;
        public LocalizationKey BodyKey;
        public bool Enabled = true;
        public int DeliveryWindowStartHour = 10;
        public int DeliveryWindowEndHour = 20;
        public int DelayHours = 24;
    }
}
