using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.UIServiceModule.Scripts;

namespace Feature.GameViewModule.Scripts {
    public class GamePresenter : PresenterBase<GameView> {
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;

        public GamePresenter(GameView view, IViewService viewService, IGameStateMachine gameStateMachine) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
        }

        private void ShowConfirmExitToMainMenu() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }
        
    }
}