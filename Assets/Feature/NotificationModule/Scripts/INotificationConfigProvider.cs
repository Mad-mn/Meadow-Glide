using Cysharp.Threading.Tasks;

namespace Feature.NotificationModule.Scripts {
    public interface INotificationConfigProvider {
        UniTask Initialize();
        NotificationEntry GetEntry(NotificationType type);
    }
}
