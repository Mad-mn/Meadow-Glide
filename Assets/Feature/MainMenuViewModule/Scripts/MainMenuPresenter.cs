using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuPresenter : PresenterBase<MainMenuView> {
        private readonly IGameStateMachine _gameStateMachine;
        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine) : base(view) {
            _gameStateMachine = gameStateMachine;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
        
        }

        private void StartSimpleGame() {
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}