using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.UIServiceModule.Scripts;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuPresenter : PresenterBase<MainMenuView> {
        private readonly IGameStateMachine _gameStateMachine;
        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine) : base(view) {
            _gameStateMachine = gameStateMachine;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
        }

        public override void Dispose() {
            base.Dispose();
            View.PlayButton.onClick.RemoveListener(StartSimpleGame);
        }

        private void StartSimpleGame() {
            View.PlayButton.onClick.RemoveListener(StartSimpleGame);
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}