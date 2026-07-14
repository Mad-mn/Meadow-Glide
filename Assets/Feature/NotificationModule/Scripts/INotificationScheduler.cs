using System;

namespace Feature.NotificationModule.Scripts {
    public interface INotificationScheduler {
        void Initialize();
        void Schedule(int id, string title, string body, DateTime fireTime);
        void Cancel(int id);
        void CancelAll();
    }
}
