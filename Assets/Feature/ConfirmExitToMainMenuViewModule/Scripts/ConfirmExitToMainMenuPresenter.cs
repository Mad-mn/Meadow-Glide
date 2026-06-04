using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.UIServiceModule.Scripts;

namespace Feature.ConfirmExitToMainMenuViewModule.Scripts {
    public class ConfirmExitToMainMenuPresenter : PresenterBase<ConfirmExitToMainMenuView> {
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;

        public ConfirmExitToMainMenuPresenter(ConfirmExitToMainMenuView view,
            IViewService viewService, IGameStateMachine gameStateMachine) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
        }

        public override void Initialize() {
            View.YesButton.onClick.AddListener(ExitToMainMenu);
            View.NoButton.onClick.AddListener(CloseConfirmWindow);
        }

        private void CloseConfirmWindow() {
            _viewService.HideView(ViewType.ConfirmExitToMainMenuView);
        }

        private void ExitToMainMenu() {
            CloseConfirmWindow();
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }
    }
}