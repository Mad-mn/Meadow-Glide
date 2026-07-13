using System;
using Unity.Notifications.Android;
using UnityEngine;

namespace Feature.NotificationModule.Scripts {
    public class AndroidNotificationScheduler : INotificationScheduler {
        public void Schedule(int id, string title, string body, DateTime fireTime) {
            var notification = new AndroidNotification {
                Title = title,
                Text = body,
                FireTime = fireTime,
                SmallIcon = "notification_icon",
                LargeIcon = "notification_large_icon",
            };
            AndroidNotificationCenter.SendNotification(notification, id.ToString());
            Debug.Log($"[Notification] Scheduled notification {id} at {fireTime}");
        }

        public void Cancel(int id) {
            AndroidNotificationCenter.CancelScheduledNotification(id);
            Debug.Log($"[Notification] Cancelled notification {id}");
        }

        public void CancelAll() {
            AndroidNotificationCenter.CancelAllScheduledNotifications();
            Debug.Log("[Notification] Cancelled all notifications");
        }
    }
}
