using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.InputModule.Scripts;
using Feature.SoundModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.ConfirmExitToMainMenuViewModule.Scripts {
    public class ConfirmExitToMainMenuPresenter : PresenterBase<ConfirmExitToMainMenuView> {
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAudioService _audioService;
        private readonly IInteractionStateService _interactionStateService;

        public ConfirmExitToMainMenuPresenter(ConfirmExitToMainMenuView view,
            IViewService viewService, IGameStateMachine gameStateMachine, IAudioService audioService,
            IInteractionStateService interactionStateService) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
            _audioService = audioService;
            _interactionStateService = interactionStateService;
        }

        public override void Initialize() {
            View.YesButton.onClick.AddListener(ExitToMainMenu);
            View.NoButton.onClick.AddListener(CloseConfirmWindow);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.InputBlocked = true;
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.InputBlocked = false;
        }

        private void CloseConfirmWindow() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.HideView(ViewType.ConfirmExitToMainMenuView);
        }

        private void ExitToMainMenu() {
            _audioService.PlaySound(AudioType.ButtonClick);
            CloseConfirmWindow();
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }
    }
}