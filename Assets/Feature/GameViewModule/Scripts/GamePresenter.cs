using Cysharp.Threading.Tasks;
using Feature.ConfirmExitToMainMenuViewModule.Scripts;
using Feature.LevelInitializeModule;
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
        private readonly ILevelInitializeService _levelInitializeService;

        public GamePresenter(GameView view, IViewService viewService, SaveDataModel saveDataModel,
            MoveTrackModel moveTrackModel, IAudioService audioService, ILevelInitializeService levelInitializeService) : base(view) {
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _moveTrackModel = moveTrackModel;
            _audioService = audioService;
            _levelInitializeService = levelInitializeService;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
            View.ResetLevelButton.onClick.AddListener(ResetLevelButtonClicked);
            _moveTrackModel.OnMovesChanged += UpdateMovesChangedsText;
        }

        private void ResetLevelButtonClicked() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _levelInitializeService.ReloadScene().Forget();
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