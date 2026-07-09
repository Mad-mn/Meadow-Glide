using Feature.InputModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.MainTutorialViewModule.Scripts {
    public class MainTutorialPresenter : PresenterBase<MainTutorialView> {
        private readonly IViewService _viewService;
        private readonly IInteractionStateService _interactionStateService;

        public MainTutorialPresenter(MainTutorialView view, IViewService viewService,
            IInteractionStateService interactionStateService) : base(view) {
            _viewService = viewService;
            _interactionStateService = interactionStateService;
        }

        private int _index;
        public override void Initialize() {
            View.Button.onClick.AddListener(OnTap);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();

            ShowStep();
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
        }

        private void OnTap() {
            ShowStep();
        }

        private void ShowStep() {
            View.First.SetActive(_index == 0);
            View.Second.SetActive(_index == 1);
            View.Third.SetActive(_index == 2);
            
            if(_index==3) {
                Close();
                _index = 0;
                return;
            }

            _index++;
        }

        private void Close() {
            _viewService.HideView(ViewType.MainTutorialView);
        }
    }
}