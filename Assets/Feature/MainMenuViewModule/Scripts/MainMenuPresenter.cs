using Feature.DebugViewModule.Scripts;
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
        private readonly IViewService _viewService;

        public MainMenuPresenter(MainMenuView view, IGameStateMachine gameStateMachine, SaveDataModel saveDataModel,
            IViewService viewService) : base(view) {
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
            _viewService = viewService;
        }

        public override void Initialize() {
            View.PlayButton.onClick.AddListener(StartSimpleGame);
            View.DebugButton.onClick.AddListener(ShowDebugWindow);
        }

        private void ShowDebugWindow() {
            _viewService.ShowView<DebugView>(ViewType.DebugView);
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