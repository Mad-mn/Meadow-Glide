using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.LevelInitializeModule;
using Feature.UIServiceModule.Scripts;
using Cysharp.Threading.Tasks;
using Feature.InputModule.Scripts;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevelPresenter : PresenterBase<WinLevel> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IInteractionStateService _interactionStateService;

        public WinLevelPresenter(WinLevel view, IGameStateMachine gameStateMachine, ILevelInitializeService levelInitializeService,
            IInteractionStateService interactionStateService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _levelInitializeService = levelInitializeService;
            _interactionStateService = interactionStateService;
        }

        public override void Initialize() {
            View.NextButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.InputBlocked = true;
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.InputBlocked = false;
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _levelInitializeService.ReloadScene().Forget();
        }
    }
}