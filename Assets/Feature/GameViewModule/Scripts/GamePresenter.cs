using Cysharp.Threading.Tasks;
using Feature.ChallengeModule.Scripts;
using Feature.ConfirmExitToMainMenuViewModule.Scripts;
using Feature.LevelInitializeModule;
using Feature.LocalizationModule.Scripts;
using Feature.LocalizationModule.Scripts.Data;
using Feature.SaveDataModule.Scripts;
using Feature.SaveDataModule.Scripts.SavedData;
using Feature.SoundModule.Scripts;
using Feature.ToolButtonViewModule.Scripts;
using Feature.TrackMoveModule.Scripts;
using Feature.UIServiceModule.Scripts;
using Feature.UndoModule.Scripts;

namespace Feature.GameViewModule.Scripts {
    public class GamePresenter : PresenterBase<GameView> {
        private readonly IViewService _viewService;
        private readonly SaveDataModel _saveDataModel;
        private readonly MoveTrackModel _moveTrackModel;
        private readonly IAudioService _audioService;
        private readonly ILevelInitializeService _levelInitializeService;
        private readonly IChallengeService _challengeService;
        private readonly ILocalizationService _localizationService;

        public GamePresenter(GameView view, IViewService viewService, SaveDataModel saveDataModel,
            MoveTrackModel moveTrackModel, IAudioService audioService, ILevelInitializeService levelInitializeService,
            IChallengeService challengeService, ILocalizationService localizationService) : base(view) {
            _viewService = viewService;
            _saveDataModel = saveDataModel;
            _moveTrackModel = moveTrackModel;
            _audioService = audioService;
            _levelInitializeService = levelInitializeService;
            _challengeService = challengeService;
            _localizationService = localizationService;
        }

        public override void Initialize() {
            View.MainMenuButton.onClick.AddListener(ShowConfirmExitToMainMenu);
            View.ResetLevelButton.onClick.AddListener(ResetLevelButtonClicked);
            _moveTrackModel.OnMovesChanged += UpdateMovesChangedsText;
            _viewService.PrewarmView<ToolButtonView>(ViewType.ToolButtonView);
        }

        public override void Dispose() {
            base.Dispose();
            _viewService.ReleasePrewarmedView(ViewType.ToolButtonView);
        }

        public override void Show() {
            _viewService.ShowView<ToolButtonView>(ViewType.ToolButtonView);
            SetupText();
        }

        public override void Hide() {
            base.Hide();
            _viewService.HideView(ViewType.ToolButtonView);
        }

        private void ResetLevelButtonClicked() {
            _audioService.PlaySound(AudioType.ButtonClick);
            _levelInitializeService.ReloadScene().Forget();
        }

        private void SetupText() {
            string lvlText = _challengeService.IsActive ?
                _localizationService.Get(LocalizationKey.DailyChallenge_Title) :
                $"{_localizationService.Get(LocalizationKey.Global_Level)} {_saveDataModel.Get<PlayerProgressData>(SaveDataType.PlayerProgress).Level}";
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