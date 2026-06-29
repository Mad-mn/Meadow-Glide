using System.Threading;
using Cysharp.Threading.Tasks;
using Feature.LocalizationModule.Scripts.Data;
using Feature.UIServiceModule.Scripts;

namespace Feature.MessageViewModule.Scripts {
    public class MessageService : IMessageService {
        private const float AUTO_HIDE_DELAY_SECONDS = 4f;

        private readonly IViewService _viewService;
        private readonly MessageViewModel _viewModel;
        private CancellationTokenSource _autoHideCts;

        public MessageService(IViewService viewService, MessageViewModel viewModel) {
            _viewService = viewService;
            _viewModel = viewModel;
        }

        public void Show(LocalizationKey message) {
            _autoHideCts?.Cancel();
            _autoHideCts?.Dispose();
            _autoHideCts = new CancellationTokenSource();

            _viewModel.SetMessage(message);
            _viewService.ShowView<MessageView>(ViewType.MessageView);
            AutoHideAsync(_autoHideCts.Token).Forget();
        }

        private async UniTaskVoid AutoHideAsync(CancellationToken ct) {
            await UniTask.Delay(System.TimeSpan.FromSeconds(AUTO_HIDE_DELAY_SECONDS), cancellationToken: ct);
            _viewModel.Hide();
        }
    }
}
