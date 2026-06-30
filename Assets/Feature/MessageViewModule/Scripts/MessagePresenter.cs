using Cysharp.Threading.Tasks;
using DG.Tweening;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.UIServiceModule.Scripts;
using UnityEngine;
using UnityEngine.UI;

namespace Feature.MessageViewModule.Scripts {
    public class MessagePresenter : PresenterBase<MessageView> {
        private const float DURATION = 0.5f;
        private readonly MessageViewModel _viewModel;
        private readonly ILocalizationService _localizationService;
        private readonly IViewService _viewService;

        public MessagePresenter(MessageView view, MessageViewModel viewModel, ILocalizationService localizationService,
            IViewService viewService) : base(view) {
            _viewModel = viewModel;
            _localizationService = localizationService;
            _viewService = viewService;
        }

        public override void Initialize() {
            _viewModel.OnMessageRequested += OnMessageRequested;
            _viewModel.HideRequested += OnHideRequested;
        }

        private void OnHideRequested() {
            View.Mask.fillOrigin = 1;
            View.Mask.DOFillAmount(0, DURATION)
                .OnComplete(HideView);
        }

        private void HideView() {
            _viewService.HideView(ViewType.MessageView);
        }

        public override void Dispose() {
            base.Dispose();
            _viewModel.OnMessageRequested -= OnMessageRequested;
        }

        public override void Show() {
            base.Show();
            LocalizationKey messageKey = _viewModel.ConsumePending();
            string message = _localizationService.Get(messageKey);
            View.SetMessage(message);
           ShowWithDelay().Forget();
        }

        private async UniTaskVoid ShowWithDelay() {
            await UniTask.Yield();
            RebuiltRectTransforms();
            View.Mask.fillOrigin = 0;
            View.Mask.DOFillAmount(1, DURATION);
        }

        private void RebuiltRectTransforms() {
            foreach (RectTransform rect in View.RectTransforms) {
                LayoutRebuilder.ForceRebuildLayoutImmediate(rect);
            }
        }

        private void OnMessageRequested(LocalizationKey messageKey) {
            string message = _localizationService.Get(messageKey);
            View.SetMessage(message);
        }
    }
}
