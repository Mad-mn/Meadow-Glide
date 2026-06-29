using Feature.LocalizationModule.Scripts.Data;

namespace Feature.MessageViewModule.Scripts {
    public interface IMessageService {
        void Show(LocalizationKey message);
    }
}
