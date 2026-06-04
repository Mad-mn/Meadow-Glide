using Feature.ConfirmExitToMainMenuViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.GameStateModule.Scripts.States;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using UnityEngine;

namespace Feature.GameViewModule.Scripts {
    public class GamePresenter : PresenterBase<GameView> {
        private readonly IViewService _viewService;
        private readonly IGameStateMachine _gameStateMachine;
        private readonly SaveDataModel _saveDataModel;
        private readonly MoveTrackModel _moveTrackModel;

        public GamePresenter(GameView view, IViewService viewService, IGameStateMachine gameStateMachine,
            SaveDataModel saveDataModel, MoveTrackModel moveTrackModel) : base(view) {
            _viewService = viewService;
            _gameStateMachine = gameStateMachine;
            _saveDataModel = saveDataModel;
            _moveTrackModel = moveTrackModel;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
            _moveTrackModel.OnMove += UpdateMovesText;
        }

        public override void Show() {
            SetupText();
        }

        private void SetupText() {
            string lvlText = $"Level {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}";
            View.SetLevelText(lvlText);
            UpdateMovesText();
        }

        private void UpdateMovesText() {
            View.SetMoveCount(_moveTrackModel.MovesLeft.ToString());
        }

        private void ShowConfirmExitToMainMenu() {
            _viewService.ShowView<ConfirmExitToMainMenuView>(ViewType.ConfirmExitToMainMenuView);
        }
    }
}