using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.MainMenuViewModule.Scripts {
    public class MainMenuPresenter : PresenterBase<MainMenuView> {
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SaveDataModel _saveDataModel;

        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine, SaveDataModel saveDataModel) : base(view) {
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
        }

        public override void Show() {
            base.Show();
            SetupText();
        }

        private void SetupText() {
            View.LevelText($"LEVEL {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}");
        }

        private void StartSimpleGame() {
            _gameStateMachine.EnterState(typeof(GameSimpleState));
        }
    }
}