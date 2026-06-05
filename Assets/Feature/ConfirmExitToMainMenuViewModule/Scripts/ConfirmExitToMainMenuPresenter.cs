using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.SoundModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.ConfirmExitToMainMenuViewModule.Scripts {
    public class ConfirmExitToMainMenuPresenter : PresenterBase<ConfirmExitToMainMenuView> {
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly IAudioService _audioService;

        public ConfirmExitToMainMenuPresenter(ConfirmExitToMainMenuView view,
            IViewService viewService, IGameStateMachine gameStateMachine, IAudioService audioService) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
            _audioService = audioService;
        }

        public override void Initialize() {
            View.YesButton.onClick.AddListener(ExitToMainMenu);
            View.NoButton.onClick.AddListener(CloseConfirmWindow);
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