using Feature.ChallengeModule.Scripts;
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
        private readonly IChallengeService _challengeService;

        public ConfirmExitToMainMenuPresenter(ConfirmExitToMainMenuView view,
            IViewService viewService, IGameStateMachine gameStateMachine, IAudioService audioService,
            IInteractionStateService interactionStateService, IChallengeService challengeService) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
            _audioService = audioService;
            _interactionStateService = interactionStateService;
            _challengeService = challengeService;
        }

        public override void Initialize() {
            View.YesButton.onClick.AddListener(ExitToMainMenu);
            View.NoButton.onClick.AddListener(CloseConfirmWindow);
        }

        public override void Show() {
            base.Show();
            _interactionStateService.BlockInput();
        }

        public override void Hide() {
            base.Hide();
            _interactionStateService.UnblockInput();
        }

        private void CloseConfirmWindow() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.HideView(ViewType.ConfirmExitToMainMenuView);
        }

        private void ExitToMainMenu() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _challengeService.Deactivate();
            CloseConfirmWindow();
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }
    }
}