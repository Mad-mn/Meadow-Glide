using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.UIServiceModule.Scripts;

namespace Feature.TutorialViewModule.Scripts {
    public class TutorialPresenter : PresenterBase<TutorialView> {
        private readonly ILocalizationService _localizationService;
        private readonly TutorialViewModel _viewModel;

        public TutorialPresenter(TutorialView view, ILocalizationService localizationService,
            TutorialViewModel viewModel) : base(view) {
            _localizationService = localizationService;
            _viewModel = viewModel;
        }

        public override void Initialize() {
            _viewModel.OnTextRequested += HandleTextRequested;

            LocalizationKey pending = _viewModel.ConsumePending(out int zone);
            if (pending != LocalizationKey.None)
                HandleTextRequested(pending, zone);
        }

        public override void Dispose() {
            base.Dispose();
            _viewModel.OnTextRequested -= HandleTextRequested;
        }

        private void HandleTextRequested(LocalizationKey key, int zone) {
            View.SetTutorialText(_localizationService.Get(key), zone);
        }
    }
}
