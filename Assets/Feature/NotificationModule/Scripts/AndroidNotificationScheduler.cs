using System;
using Unity.Notifications.Android;
using UnityEngine;

namespace Feature.NotificationModule.Scripts {
    public class AndroidNotificationScheduler : INotificationScheduler {
        
        private bool _initialized;
        public void Initialize() {
            if(_initialized) return;
            var channel = new AndroidNotificationChannel()
            {
                Id = "default_channel",
                Name = "Default Channel",
                Importance = Importance.High,
                Description = "Game notifications",
            };

            AndroidNotificationCenter.RegisterNotificationChannel(channel);
            _initialized = true;
        }

        public void Schedule(int id, string title, string body, DateTime fireTime) {
            if (!_initialized) {
                Initialize();
            }
            var notification = new AndroidNotification {
                Title = title,
                Text = body,
                FireTime = fireTime,
                SmallIcon = "notification_icon",
                LargeIcon = "notification_large_icon",
            };
            AndroidNotificationCenter.SendNotification(notification, "default_channel");
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
