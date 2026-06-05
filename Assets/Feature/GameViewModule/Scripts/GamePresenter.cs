using Feature.ConfirmExitToMainMenuViewModule.Scripts;
using Feature.GameStateModule.Scripts;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SoundModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;

namespace Feature.GameViewModule.Scripts {
    public class GamePresenter : PresenterBase<GameView> {
        private readonly IViewService _viewService;
        private readonly SaveDataModel _saveDataModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IAudioService _audioService;

        public GamePresenter(GameView view, IViewService viewService, SaveDataModel saveDataModel,
            MoveTrackModel moveTrackModel, IAudioService audioService) : base(view) {
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _moveTrackModel = moveTrackModel;
            _audioService = audioService;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
            _moveTrackModel.OnMovesChanged += UpdateMovesChangedsText;
        }

        public override void Show() {
            SetupText();
        }

        private void SetupText() {
            string lvlText = $"Level {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}";
            View.SetLevelText(lvlText);
            UpdateMovesChangedsText();
        }

        private void UpdateMovesChangedsText() {
            View.SetMoveCount(_moveTrackModel.MovesLeft.ToString());
        }

        private void ShowConfirmExitToMainMenu() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _viewService.ShowView<ConfirmExitToMainMenuView>(ViewType.ConfirmExitToMainMenuView);
        }
    }
}