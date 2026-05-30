using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.UIServiceModule.Scripts;

namespace Feature.WinLevelModule.Scripts {
    public class WinLevelPresenter : PresenterBase<WinLevel> {
        private readonly IGameStateMachine _gameStateMachine;
        public WinLevelPresenter(WinLevel view, IGameStateMachine gameStateMachine) : base(view) {
            _gameStateMachine = gameStateMachine;
        }

        public override void Initialize() {
            View.NextButton.onClick.AddListener(OnNextButtonClick);
            View.MainMenuButton.onClick.AddListener(OnMainMenuButtonClick);
        }

        private void OnMainMenuButtonClick() {
            _gameStateMachine.EnterState(typeof(MainMenuState));
        }

        private void OnNextButtonClick() {
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}